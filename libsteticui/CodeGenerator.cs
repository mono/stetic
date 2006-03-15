using System;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace Stetic
{
	public static class CodeGenerator
	{
		static CodeExpression bindingFlags;
		
		static CodeGenerator ()
		{
			CodeTypeReferenceExpression flagsType = new CodeTypeReferenceExpression ("System.Reflection.BindingFlags");
			bindingFlags = new CodeBinaryOperatorExpression (
				new CodeFieldReferenceExpression (flagsType, "Public"),
				CodeBinaryOperatorType.BitwiseOr,
				new CodeFieldReferenceExpression (flagsType, "NonPublic")
			);
			
			bindingFlags = new CodeBinaryOperatorExpression (
				bindingFlags,
				CodeBinaryOperatorType.BitwiseOr,
				new CodeFieldReferenceExpression (flagsType, "Instance")
			);		
		}
	
		public static void GenerateProjectCode (string file, string namespaceName, CodeDomProvider provider, params Project[] projects)
		{
			CodeCompileUnit cunit = new CodeCompileUnit ();
			CodeNamespace cns = new CodeNamespace (namespaceName);
			cunit.Namespaces.Add (cns);
			GenerateProjectCode (cns, projects);
			
			ICodeGenerator gen = provider.CreateGenerator ();
			StreamWriter fileStream = new StreamWriter (file);
			try {
				gen.GenerateCodeFromCompileUnit (cunit, fileStream, new CodeGeneratorOptions ());
			} finally {
				fileStream.Close ();
			}
		}
		
		public static void GenerateProjectCode (CodeNamespace cns, params Project[] projects)
		{
			bool multiProject = projects.Length > 1;
			
			CodeTypeDeclaration type = new CodeTypeDeclaration ("Gui");
			type.Attributes = MemberAttributes.Private;
			type.TypeAttributes = TypeAttributes.NestedAssembly;
			cns.Types.Add (type);
			
			// Buid method overload that takes a type as parameter.
			
			CodeMemberMethod met = new CodeMemberMethod ();
			met.Name = "Build";
			type.Members.Add (met);
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "obj"));
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(Type), "type"));
			if (multiProject)
				met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "file"));
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			
			CodeMethodInvokeExpression call = new CodeMethodInvokeExpression (
					new CodeMethodReferenceExpression (
						new CodeTypeReferenceExpression (cns.Name + ".Gui"),
						"Build"
					),
					new CodeVariableReferenceExpression ("obj"),
					new CodePropertyReferenceExpression (
						new CodeVariableReferenceExpression ("type"),
						"FullName"
					)
			);
			if (multiProject)
				call.Parameters.Add (new CodeVariableReferenceExpression ("file"));
				
			met.Statements.Add (call);
			
			// Generate the build method
			
			met = new CodeMemberMethod ();
			met.Name = "Build";
			type.Members.Add (met);
			
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "obj"));
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "id"));
			if (multiProject)
				met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "file"));
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			
			CodeVariableReferenceExpression cobj = new CodeVariableReferenceExpression ("obj");
			CodeVariableReferenceExpression cfile = new CodeVariableReferenceExpression ("file");
			CodeVariableReferenceExpression cid = new CodeVariableReferenceExpression ("id");
			
			CodeVariableDeclarationStatement varDecHash = new CodeVariableDeclarationStatement (typeof(System.Collections.Hashtable), "widgets");
			met.Statements.Add (varDecHash);
			varDecHash.InitExpression = new CodeObjectCreateExpression (
				typeof(System.Collections.Hashtable),
				new CodeExpression [0]
			);
			
			CodeStatementCollection projectCol = met.Statements;
			
			foreach (Project gp in projects) {
			
				CodeStatementCollection widgetCol;
				
				if (multiProject) {
					CodeConditionStatement pcond = new CodeConditionStatement ();
					pcond.Condition = new CodeBinaryOperatorExpression (
						cfile, 
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (gp.Id)
					);
					projectCol.Add (pcond);
					
					widgetCol = pcond.TrueStatements;
					projectCol = pcond.FalseStatements;
				} else {
					widgetCol = projectCol;
				}
				
				foreach (Gtk.Widget w in gp.Toplevels) {
					CodeConditionStatement cond = new CodeConditionStatement ();
					cond.Condition = new CodeBinaryOperatorExpression (
						cid, 
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (w.Name)
					);
					widgetCol.Add (cond);
					
					Stetic.Wrapper.Widget wwidget = Stetic.Wrapper.Widget.Lookup (w);
					
					CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (wwidget.ClassDescriptor.WrappedTypeName, "cobj");
					varDec.InitExpression = new CodeCastExpression (wwidget.ClassDescriptor.WrappedTypeName, cobj);
					cond.TrueStatements.Add (varDec);

					Stetic.WidgetMap map = Stetic.CodeGenerator.GenerateBuildCode (cns, w, "cobj", cond.TrueStatements);
					
					BindSignalHandlers (wwidget, wwidget, map, cond.TrueStatements);
					
					widgetCol = cond.FalseStatements;
				}
			}
			
			// Bind the fields
			
			CodeVariableDeclarationStatement varDecIndex = new CodeVariableDeclarationStatement (typeof(int), "n");
			varDecIndex.InitExpression = new CodePrimitiveExpression (0);
			CodeExpression varIndex = new CodeVariableReferenceExpression ("n");
			
			CodeVariableDeclarationStatement varDecArray = new CodeVariableDeclarationStatement (typeof(FieldInfo[]), "fields");
			varDecArray.InitExpression = new CodeMethodInvokeExpression (
				new CodeMethodInvokeExpression (
					cobj,
					"GetType",
					new CodeExpression [0]
				),
				"GetFields",
				bindingFlags
			);
			met.Statements.Add (varDecArray);
			CodeVariableReferenceExpression varArray = new CodeVariableReferenceExpression ("fields");
			
			CodeIterationStatement iteration = new CodeIterationStatement ();
			met.Statements.Add (iteration);
			
			iteration.InitStatement = varDecIndex;
			
			iteration.TestExpression = new CodeBinaryOperatorExpression (
				varIndex,
				CodeBinaryOperatorType.LessThan,
				new CodePropertyReferenceExpression (varArray, "Length")
			);
			iteration.IncrementStatement = new CodeAssignStatement (
				varIndex,
				new CodeBinaryOperatorExpression (
					varIndex,
					CodeBinaryOperatorType.Add,
					new CodePrimitiveExpression (1)
				)
			);
			
			CodeVariableDeclarationStatement varDecField = new CodeVariableDeclarationStatement (typeof(FieldInfo), "field");
			varDecField.InitExpression = new CodeArrayIndexerExpression (varArray, new CodeExpression [] {varIndex});
			CodeVariableReferenceExpression varField = new CodeVariableReferenceExpression ("field");
			iteration.Statements.Add (varDecField);
			
			CodeVariableDeclarationStatement varDecWidget = new CodeVariableDeclarationStatement (typeof(object), "widget");
			iteration.Statements.Add (varDecWidget);
			varDecWidget.InitExpression = new CodeIndexerExpression (
				new CodeVariableReferenceExpression ("widgets"),
				new CodePropertyReferenceExpression (varField, "Name")
			);
			CodeVariableReferenceExpression varWidget = new CodeVariableReferenceExpression ("widget");
			
			// Make sure the type of the field matches the type of the widget
			
			CodeConditionStatement fcond = new CodeConditionStatement ();
			iteration.Statements.Add (fcond);
			fcond.Condition = new CodeBinaryOperatorExpression (
				new CodeBinaryOperatorExpression (
					varWidget,
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression (null)
				),
				CodeBinaryOperatorType.BooleanAnd,
				new CodeMethodInvokeExpression (
					new CodePropertyReferenceExpression (varField, "FieldType"),
					"IsInstanceOfType",
					varWidget
				)
			);
			
			// Set the variable value
			
			fcond.TrueStatements.Add (
				new CodeMethodInvokeExpression (
					varField,
					"SetValue",
					cobj,
					varWidget
				)
			);
		}
		
		static void BindSignalHandlers (Stetic.Wrapper.Widget rootWidget, Stetic.Wrapper.Widget widget, Stetic.WidgetMap map, CodeStatementCollection statements)
		{
			foreach (Stetic.Wrapper.Signal signal in widget.Signals) {
				TypedSignalDescriptor descriptor = signal.SignalDescriptor as TypedSignalDescriptor;
				if (descriptor == null) continue;
				
				CodeExpression createDelegate =
					new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression (typeof(Delegate)),
						"CreateDelegate",
						new CodeTypeOfExpression (descriptor.HandlerTypeName),
						new CodeVariableReferenceExpression (map.GetWidgetId (rootWidget.Wrapped.Name)),
						new CodePrimitiveExpression (signal.Handler));
				
				createDelegate = new CodeCastExpression (descriptor.HandlerTypeName, createDelegate);
				
				CodeAttachEventStatement cevent = new CodeAttachEventStatement (
					new CodeEventReferenceExpression (
						new CodeVariableReferenceExpression (map.GetWidgetId (widget.Wrapped.Name)),
						descriptor.Name),
					createDelegate);
				
				statements.Add (cevent);
			}
			
			Gtk.Container cont = widget.Wrapped as Gtk.Container;
			if (cont != null) {
				foreach (Gtk.Widget child in cont.AllChildren) {
					Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (child);
					if (ww != null)
						BindSignalHandlers (rootWidget, ww, map, statements);
				}
			}
		}
		
		public static WidgetMap GenerateBuildCode (CodeNamespace cns, Gtk.Widget w, string widgetVarName, CodeStatementCollection statements)
		{
			statements.Add (new CodeCommentStatement ("Widget " + w.Name));
			GeneratorContext ctx = new ProjectGeneratorContext (cns, statements);
			Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (w);
			ctx.GenerateBuildCode (ww, widgetVarName);
			ctx.EndGeneration ();
			return ctx.WidgetMap;
		}
	}
	
	class ProjectGeneratorContext: GeneratorContext
	{
		public ProjectGeneratorContext (CodeNamespace cns, CodeStatementCollection statements): base (cns, "w", statements)
		{
		}
		
		public override void GenerateBuildCode (Wrapper.Widget widget, string varName)
		{
			base.GenerateBuildCode (widget, varName);
			Statements.Add (
				new CodeAssignStatement (
					new CodeIndexerExpression (
						new CodeVariableReferenceExpression ("widgets"),
						new CodePrimitiveExpression (widget.Wrapped.Name)
					),
					new CodeVariableReferenceExpression (varName)
				)
			);
		}
	}
}

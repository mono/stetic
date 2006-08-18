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
			GenerateProjectGuiCode (cns, projects);
			GenerateProjectActionsCode (cns, projects);
		}
		
		static void GenerateProjectGuiCode (CodeNamespace cns, params Project[] projects)
		{
			bool multiProject = projects.Length > 1;
			
			CodeTypeDeclaration type = new CodeTypeDeclaration ("Gui");
			type.Attributes = MemberAttributes.Private;
			type.TypeAttributes = TypeAttributes.NestedAssembly;
			cns.Types.Add (type);
			
			// Create the project initialization method
			// This method will only be added at the end if there
			// is actually something to initialize
			
			CodeMemberMethod initMethod = new CodeMemberMethod ();
			initMethod.Name = "Initialize";
			initMethod.ReturnType = new CodeTypeReference (typeof(void));
			initMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			GeneratorContext initContext = new ProjectGeneratorContext (cns, initMethod.Statements);
			
			// Build method overload that takes a type as parameter.
			
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
					new CodeArgumentReferenceExpression ("obj"),
					new CodePropertyReferenceExpression (
						new CodeArgumentReferenceExpression ("type"),
						"FullName"
					)
			);
			if (multiProject)
				call.Parameters.Add (new CodeArgumentReferenceExpression ("file"));

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
			
			CodeArgumentReferenceExpression cobj = new CodeArgumentReferenceExpression ("obj");
			CodeArgumentReferenceExpression cfile = new CodeArgumentReferenceExpression ("file");
			CodeArgumentReferenceExpression cid = new CodeArgumentReferenceExpression ("id");
			
			CodeVariableDeclarationStatement varDecHash = new CodeVariableDeclarationStatement (typeof(System.Collections.Hashtable), "bindings");
			met.Statements.Add (varDecHash);
			varDecHash.InitExpression = new CodeObjectCreateExpression (
				typeof(System.Collections.Hashtable),
				new CodeExpression [0]
			);
			
			CodeStatementCollection projectCol = met.Statements;
			
			// Generate code for each project
			
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
				
				// Generate widget factory creation
				
				if (gp.IconFactory.Icons.Count > 0)
					gp.IconFactory.GenerateBuildCode (initContext);
				
				// Generate top levels
				
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

					Stetic.WidgetMap map = Stetic.CodeGenerator.GenerateCreationCode (cns, w, "cobj", cond.TrueStatements);
					
					CodeVariableReferenceExpression targetObjectVar = new CodeVariableReferenceExpression ("cobj");
					BindSignalHandlers (targetObjectVar, wwidget, map, cond.TrueStatements);
					
					widgetCol = cond.FalseStatements;
				}
				
				// Generate action groups
				
				foreach (Wrapper.ActionGroup agroup in gp.ActionGroups) {
					CodeConditionStatement cond = new CodeConditionStatement ();
					cond.Condition = new CodeBinaryOperatorExpression (
						cid, 
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (agroup.Name)
					);
					widgetCol.Add (cond);
					
					CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement ("Gtk.ActionGroup", "cobj");
					varDec.InitExpression = new CodeCastExpression ("Gtk.ActionGroup", cobj);
					cond.TrueStatements.Add (varDec);
					
					Stetic.WidgetMap map = Stetic.CodeGenerator.GenerateCreationCode (cns, agroup, "cobj", cond.TrueStatements);
					
					CodeVariableReferenceExpression targetObjectVar = new CodeVariableReferenceExpression ("cobj");
					foreach (Wrapper.Action ac in agroup.Actions)
						BindSignalHandlers (targetObjectVar, ac, map, cond.TrueStatements);
					
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
				new CodeVariableReferenceExpression ("bindings"),
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
			
			// Final step. If there is some initialization code, add all needed infrastructure
			
			if (initMethod.Statements.Count > 0) {
				type.Members.Add (initMethod);
				
				CodeMemberField initField = new CodeMemberField (typeof(bool), "initialized");
				initField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
				type.Members.Add (initField);
				
				CodeFieldReferenceExpression initVar = new CodeFieldReferenceExpression (
					new CodeTypeReferenceExpression (cns.Name + ".Gui"),
					"initialized"
				);
				
				CodeConditionStatement initCondition = new CodeConditionStatement ();
				initCondition.Condition = new CodeBinaryOperatorExpression (
					initVar, 
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression (false)
				);
				initCondition.TrueStatements.Add (
					new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression (cns.Name + ".Gui"),
						"Initialize"
					)
				);
				met.Statements.Insert (0, initCondition);
			}
		}
		
		static void BindSignalHandlers (CodeExpression targetObjectVar, ObjectWrapper wrapper, Stetic.WidgetMap map, CodeStatementCollection statements)
		{
			foreach (Signal signal in wrapper.Signals) {
				TypedSignalDescriptor descriptor = signal.SignalDescriptor as TypedSignalDescriptor;
				if (descriptor == null) continue;
				
				CodeExpression createDelegate =
					new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression (typeof(Delegate)),
						"CreateDelegate",
						new CodeTypeOfExpression (descriptor.HandlerTypeName),
						targetObjectVar,
						new CodePrimitiveExpression (signal.Handler));
				
				createDelegate = new CodeCastExpression (descriptor.HandlerTypeName, createDelegate);
				
				CodeAttachEventStatement cevent = new CodeAttachEventStatement (
					new CodeEventReferenceExpression (
						new CodeVariableReferenceExpression (map.GetWidgetId (wrapper)),
						descriptor.Name),
					createDelegate);
				
				statements.Add (cevent);
			}
			
			Wrapper.Widget widget = wrapper as Wrapper.Widget;
			if (widget != null && widget.IsTopLevel) {
				// Bind local action signals
				foreach (Wrapper.ActionGroup grp in widget.LocalActionGroups) {
					foreach (Wrapper.Action ac in grp.Actions)
						BindSignalHandlers (targetObjectVar, ac, map, statements);
				}
			}
			
			Gtk.Container cont = wrapper.Wrapped as Gtk.Container;
			if (cont != null) {
				foreach (Gtk.Widget child in cont.AllChildren) {
					Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (child);
					if (ww != null)
						BindSignalHandlers (targetObjectVar, ww, map, statements);
				}
			}
			
		}
		
		static void GenerateProjectActionsCode (CodeNamespace cns, params Project[] projects)
		{
			bool multiProject = projects.Length > 1;
			
			CodeTypeDeclaration type = new CodeTypeDeclaration ("ActionGroups");
			type.Attributes = MemberAttributes.Private;
			type.TypeAttributes = TypeAttributes.NestedAssembly;
			cns.Types.Add (type);

			// Generate the global action group getter
			
			CodeMemberMethod met = new CodeMemberMethod ();
			met.Name = "GetActionGroup";
			type.Members.Add (met);
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(Type), "type"));
			if (multiProject)
				met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "file"));
			met.ReturnType = new CodeTypeReference (typeof(Gtk.ActionGroup));
			met.Attributes = MemberAttributes.Public | MemberAttributes.Static;

			CodeMethodInvokeExpression call = new CodeMethodInvokeExpression (
					new CodeMethodReferenceExpression (
						new CodeTypeReferenceExpression (cns.Name + ".ActionGroups"),
						"GetActionGroup"
					),
					new CodePropertyReferenceExpression (
						new CodeArgumentReferenceExpression ("type"),
						"FullName"
					)
			);
			if (multiProject)
				call.Parameters.Add (new CodeArgumentReferenceExpression ("file"));
				
			met.Statements.Add (new CodeMethodReturnStatement (call));

			// Generate the global action group getter (overload)
			
			met = new CodeMemberMethod ();
			met.Name = "GetActionGroup";
			type.Members.Add (met);
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "name"));
			if (multiProject)
				met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(string), "file"));
			met.ReturnType = new CodeTypeReference (typeof(Gtk.ActionGroup));
			met.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			
			CodeArgumentReferenceExpression cfile = new CodeArgumentReferenceExpression ("file");
			CodeArgumentReferenceExpression cid = new CodeArgumentReferenceExpression ("name");
			
			CodeStatementCollection projectCol = met.Statements;
			int n=1;
			
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
				
				foreach (Wrapper.ActionGroup grp in gp.ActionGroups) {
					string fname = "group" + (n++);
					CodeMemberField grpField = new CodeMemberField (typeof(Gtk.ActionGroup), fname);
					grpField.Attributes |= MemberAttributes.Static;
					type.Members.Add (grpField);
					CodeFieldReferenceExpression grpVar = new CodeFieldReferenceExpression (
						new CodeTypeReferenceExpression (cns.Name + ".ActionGroups"),
						fname
					);
					
					CodeConditionStatement pcond = new CodeConditionStatement ();
					pcond.Condition = new CodeBinaryOperatorExpression (
						cid, 
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (grp.Name)
					);
					widgetCol.Add (pcond);
					
					// If the group has not yet been created, create it
					CodeConditionStatement pcondGrp = new CodeConditionStatement ();
					pcondGrp.Condition = new CodeBinaryOperatorExpression (
						grpVar, 
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (null)
					);
					
					pcondGrp.TrueStatements.Add (
						new CodeAssignStatement (
							grpVar,
							new CodeObjectCreateExpression (grp.Name)
						)
					);
					
					pcond.TrueStatements.Add (pcondGrp);
					pcond.TrueStatements.Add (new CodeMethodReturnStatement (grpVar));
					
					widgetCol = pcond.FalseStatements;
				}
				widgetCol.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (null)));
			}
		}
		
		public static WidgetMap GenerateCreationCode (CodeNamespace cns, Gtk.Widget w, string widgetVarName, CodeStatementCollection statements)
		{
			statements.Add (new CodeCommentStatement ("Widget " + w.Name));
			GeneratorContext ctx = new ProjectGeneratorContext (cns, statements);
			Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (w);
			ctx.GenerateCreationCode (ww, widgetVarName);
			ctx.EndGeneration ();
			return ctx.WidgetMap;
		}
		
		public static WidgetMap GenerateCreationCode (CodeNamespace cns, Wrapper.ActionGroup grp, string groupVarName, CodeStatementCollection statements)
		{
			statements.Add (new CodeCommentStatement ("Action group " + grp.Name));
			GeneratorContext ctx = new ProjectGeneratorContext (cns, statements);
			ctx.GenerateCreationCode (grp, groupVarName);
			ctx.EndGeneration ();
			return ctx.WidgetMap;
		}
	}
	
	class ProjectGeneratorContext: GeneratorContext
	{
		public ProjectGeneratorContext (CodeNamespace cns, CodeStatementCollection statements): base (cns, "w", statements)
		{
		}
		
		public override void GenerateBuildCode (ObjectWrapper wrapper, string varName)
		{
			base.GenerateBuildCode (wrapper, varName);
			
			string memberName = null;
			if (wrapper is Wrapper.Widget)
				memberName = ((Wrapper.Widget) wrapper).Wrapped.Name;
			else if (wrapper is Wrapper.Action)
				memberName = ((Wrapper.Action) wrapper).Name;
				
			if (memberName != null) {
				Statements.Add (
					new CodeAssignStatement (
						new CodeIndexerExpression (
							new CodeVariableReferenceExpression ("bindings"),
							new CodePrimitiveExpression (memberName)
						),
						new CodeVariableReferenceExpression (varName)
					)
				);
			}
		}
	}
}

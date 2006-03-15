using System;
using System.CodeDom;
using System.Reflection;

namespace Stetic.Wrapper
{
	public class Bin: Container
	{
		public static new Gtk.Bin CreateInstance (ClassDescriptor klass)
		{
			if (klass.Name == "Gtk.Bin")
				return new CustomWidget ();
			else
				return null;
		}
		
		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			if (ClassDescriptor.WrappedTypeName == "Gtk.Bin") {
			
				// Gtk.Bin needs a helper class which handles child allocation.
				// This class needs to be generated since Stetic won't be linked with
				// the app.
				
				bool found = false;
				foreach (CodeTypeDeclaration dec in ctx.CodeNamespace.Types) {
					if (dec.Name == "BinContainer") {
						found = true;
						break;
					}
				}
				
				if (!found)
					GenerateHelperClass (ctx);
				
				ctx.Statements.Add (
					new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression ("BinContainer"),
						"Attach",
						new CodeVariableReferenceExpression (varName)
					)
				);
			}
			base.GenerateBuildCode (ctx, varName);
		}
		
		void GenerateHelperClass (GeneratorContext ctx)
		{
			CodeTypeDeclaration type = new CodeTypeDeclaration ("BinContainer");
			type.Attributes = MemberAttributes.Private;
			type.TypeAttributes = TypeAttributes.NestedAssembly;
			ctx.CodeNamespace.Types.Add (type);
			
			CodeMemberField field = new CodeMemberField ("Gtk.Widget", "child");
			field.Attributes = MemberAttributes.Private;
			type.Members.Add (field);
			
			CodeExpression child = new CodeFieldReferenceExpression (
				new CodeThisReferenceExpression (),
				"child"
			);
			
			// Attach method
			
			CodeMemberMethod met = new CodeMemberMethod ();
			type.Members.Add (met);
			met.Name = "Attach";
			met.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Parameters.Add (new CodeParameterDeclarationExpression ("Gtk.Bin", "bin"));
			
			CodeVariableDeclarationStatement bcDec = new CodeVariableDeclarationStatement ("BinContainer", "bc");
			bcDec.InitExpression = new CodeObjectCreateExpression ("BinContainer");
			met.Statements.Add (bcDec);
			CodeVariableReferenceExpression bc = new CodeVariableReferenceExpression ("bc");
			CodeVariableReferenceExpression bin = new CodeVariableReferenceExpression ("bin");
			
			met.Statements.Add (
				new CodeAttachEventStatement (
					bin, 
					"SizeRequested",
					new CodeDelegateCreateExpression (
						new CodeTypeReference ("Gtk.SizeRequestedHandler"), bc, "OnSizeRequested"
					)
				)
			);
			
			met.Statements.Add (
				new CodeAttachEventStatement (
					bin, 
					"SizeAllocated",
					new CodeDelegateCreateExpression (
						new CodeTypeReference ("Gtk.SizeAllocatedHandler"), bc, "OnSizeAllocated"
					)
				)
			);
			
			met.Statements.Add (
				new CodeAttachEventStatement (
					bin, 
					"Added",
					new CodeDelegateCreateExpression (
						new CodeTypeReference ("Gtk.AddedHandler"), bc, "OnAdded"
					)
				)
			);
			
			// OnSizeRequested override
			
			met = new CodeMemberMethod ();
			type.Members.Add (met);
			met.Name = "OnSizeRequested";
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "sender"));
			met.Parameters.Add (new CodeParameterDeclarationExpression ("Gtk.SizeRequestedArgs", "args"));
			
			CodeConditionStatement cond = new CodeConditionStatement ();
			cond.Condition = new CodeBinaryOperatorExpression (
						child,
						CodeBinaryOperatorType.IdentityInequality,
						new CodePrimitiveExpression (null)
			);
			cond.TrueStatements.Add (
				new CodeAssignStatement (
					new CodePropertyReferenceExpression (
						new CodeVariableReferenceExpression ("args"),
						"Requisition"
					),
					new CodeMethodInvokeExpression (
						child,
						"SizeRequest"
					)
				)
			);
			met.Statements.Add (cond);
			
			// OnSizeAllocated method
			
			met = new CodeMemberMethod ();
			type.Members.Add (met);
			met.Name = "OnSizeAllocated";
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "sender"));
			met.Parameters.Add (new CodeParameterDeclarationExpression ("Gtk.SizeAllocatedArgs", "args"));
			
			cond = new CodeConditionStatement ();
			cond.Condition = new CodeBinaryOperatorExpression (
						child,
						CodeBinaryOperatorType.IdentityInequality,
						new CodePrimitiveExpression (null)
			);
			cond.TrueStatements.Add (
				new CodeAssignStatement (
					new CodePropertyReferenceExpression (
						child,
						"Allocation"
					),
					new CodePropertyReferenceExpression (
						new CodeVariableReferenceExpression ("args"),
						"Allocation"
					)
				)
			);
			met.Statements.Add (cond);
			
			// OnAdded method
			
			met = new CodeMemberMethod ();
			type.Members.Add (met);
			met.Name = "OnAdded";
			met.ReturnType = new CodeTypeReference (typeof(void));
			met.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "sender"));
			met.Parameters.Add (new CodeParameterDeclarationExpression ("Gtk.AddedArgs", "args"));
			
			met.Statements.Add (
				new CodeAssignStatement (
					child,
					new CodePropertyReferenceExpression (
						new CodeVariableReferenceExpression ("args"),
						"Widget"
					)
				)
			);
		}
	}
	
/*
	 This is a model of what GenerateHelperClass generates:
	
	class BinContainer
	{
		Gtk.Widget child;
		
		public static void Attach (Gtk.Bin bin)
		{
			BinContainer bc = new BinContainer ();
			bin.SizeRequested += new Gtk.SizeRequestedHandler (bc.OnSizeRequested);
			bin.SizeAllocated += new Gtk.SizeAllocatedHandler (bc.OnSizeAllocated);
			bin.Added += new Gtk.AddedHandler (bc.OnAdded);
		}
		
		void OnSizeRequested (object s, Gtk.SizeRequestedArgs args)
		{
			if (child != null)
				args.Requisition = child.SizeRequest ();
		}
		
		void OnSizeAllocated (object s, Gtk.SizeAllocatedArgs args)
		{
			if (child != null)
				child.Allocation = args.Allocation;
		}
		
		void OnAdded (object s, Gtk.AddedArgs args)
		{
			child = args.Widget;
		}
	}
*/

}

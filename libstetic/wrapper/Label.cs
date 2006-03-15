using System;
using System.CodeDom;
using System.Collections;

namespace Stetic.Wrapper {

	public class Label : Widget {

		public Label () {}

		string mnem;
		public string MnemonicWidget {
			get {
				return mnem;
			}
			set {
				mnem = value;
			}
		}
		
		protected override void GeneratePropertySet (GeneratorContext ctx, CodeVariableReferenceExpression var, PropertyDescriptor prop)
		{
			if (prop.Name != "MnemonicWidget")
				base.GeneratePropertySet (ctx, var, prop);
		}
		
		internal protected override void GeneratePostBuildCode (GeneratorContext ctx, string varName)
		{
			if (mnem != null) {
				string memVar = ctx.WidgetMap.GetWidgetId (mnem);
				if (memVar != null) {
					ctx.Statements.Add (
						new CodeAssignStatement (
							new CodePropertyReferenceExpression (
								new CodeVariableReferenceExpression (varName), 
								"MnemonicWidget"
							),
							new CodeVariableReferenceExpression (memVar)
						)
					);
				}
			}
			base.GeneratePostBuildCode (ctx, varName);
		}
	}
}

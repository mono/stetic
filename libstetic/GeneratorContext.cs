using System;
using System.CodeDom;
using System.Collections;

namespace Stetic
{
	public class GeneratorContext
	{
		CodeNamespace cns;
		int n;
		string idPrefix;
		Hashtable vars = new Hashtable ();
		Hashtable widgets = new Hashtable ();
		WidgetMap map;
		
		public GeneratorContext (CodeNamespace cns, string idPrefix)
		{
			this.cns = cns;
			this.idPrefix = idPrefix;
			map = new WidgetMap (vars, widgets);
		}
		
		public CodeNamespace CodeNamespace {
			get { return cns; }
		}
		
		public string NewId ()
		{
			return idPrefix + (++n);
		}
		
		public string GenerateCreationCode (Wrapper.Widget widget, CodeStatementCollection statements)
		{
			string varName = NewId ();
			
			CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (widget.ClassDescriptor.WrappedTypeName, varName);
			statements.Add (varDec);
			varDec.InitExpression = widget.GenerateWidgetCreation (this, statements);
			GenerateBuildCode (widget, varName, statements);
			return varName;
		}
		
		public virtual void GenerateBuildCode (Wrapper.Widget widget, string varName, CodeStatementCollection statements)
		{
			vars [widget.Wrapped.Name] = varName;
			widgets [varName] = widget.Wrapped;
			widget.GenerateBuildCode (this, varName, statements);
		}
		
		public CodeExpression GenerateValue (object value)
		{
			if (value == null)
				return new CodePrimitiveExpression (value);
				
			if (value.GetType ().IsEnum) {
				long ival = (long) Convert.ChangeType (value, typeof(long));
				return new CodeCastExpression (
					new CodeTypeReference (value.GetType ()), 
					new CodePrimitiveExpression (ival)
				);
			}
			
			if (value is Gtk.Adjustment) {
				Gtk.Adjustment adj = value as Gtk.Adjustment;
				return new CodeObjectCreateExpression (
					typeof(Gtk.Adjustment),
					new CodePrimitiveExpression (adj.Value),
					new CodePrimitiveExpression (adj.Lower),
					new CodePrimitiveExpression (adj.Upper),
					new CodePrimitiveExpression (adj.StepIncrement),
					new CodePrimitiveExpression (adj.PageIncrement),
					new CodePrimitiveExpression (adj.PageSize));
			}
			if (value is ushort || value is uint) {
				return new CodeCastExpression (
					new CodeTypeReference (value.GetType ()),
					new CodePrimitiveExpression (Convert.ChangeType (value, typeof(long))));
			}
			if (value is ulong) {
				return new CodeMethodInvokeExpression (
					new CodeTypeReferenceExpression (value.GetType ()),
					"Parse",
					new CodePrimitiveExpression (value.ToString ()));
			}
			
			return new CodePrimitiveExpression (value);
		}
		
		public WidgetMap WidgetMap {
			get { return map; }
		}
		
		public void Reset ()
		{
			vars.Clear ();
			n = 0;
		}
	}

	public class WidgetMap
	{
		Hashtable vars;
		Hashtable widgets;
		
		internal WidgetMap (Hashtable vars, Hashtable widgets)
		{
			this.vars = vars;
			this.widgets = widgets;
		}
		
		public string GetWidgetId (string widgetName)
		{
			return (string) vars [widgetName];
		}
		
		public Gtk.Widget GetWidgetFromId (string widgetId)
		{
			return (Gtk.Widget) widgets [widgetId];
		}
	}
}


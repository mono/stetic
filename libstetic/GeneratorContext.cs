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
		ArrayList generatedWrappers = new ArrayList ();
		WidgetMap map;
		CodeStatementCollection statements;
		
		public GeneratorContext (CodeNamespace cns, string idPrefix, CodeStatementCollection statements)
		{
			this.cns = cns;
			this.idPrefix = idPrefix;
			this.statements = statements;
			map = new WidgetMap (vars, widgets);
		}
		
		public CodeNamespace CodeNamespace {
			get { return cns; }
		}
		
		public CodeStatementCollection Statements {
			get { return statements; }
		}
		
		public string NewId ()
		{
			return idPrefix + (++n);
		}
		
		public string GenerateCreationCode (Wrapper.Widget widget)
		{
			string varName = NewId ();
			
			CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (widget.ClassDescriptor.WrappedTypeName, varName);
			statements.Add (varDec);
			varDec.InitExpression = widget.GenerateWidgetCreation (this);
			GenerateBuildCode (widget, varName);
			return varName;
		}
		
		public virtual void GenerateBuildCode (Wrapper.Widget widget, string varName)
		{
			vars [widget.Wrapped.Name] = varName;
			widgets [varName] = widget.Wrapped;
			widget.GenerateBuildCode (this, varName);
			generatedWrappers.Add (widget);
		}
		
		public CodeExpression GenerateValue (object value, Type type)
		{
			if (value == null)
				return new CodePrimitiveExpression (value);
				
			if (value.GetType ().IsEnum) {
				if (!type.IsEnum) {
					object ival = Convert.ChangeType (value, type);
					return new CodePrimitiveExpression (ival);
				} else {
					long ival = (long) Convert.ChangeType (value, typeof(long));
					return new CodeCastExpression (
						new CodeTypeReference (value.GetType ()), 
						new CodePrimitiveExpression (ival)
					);
				}
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
			
			if (value is ImageInfo && typeof(Gdk.Pixbuf).IsAssignableFrom (type))
				return ((ImageInfo)value).ToCodeExpression ();
			
			return new CodePrimitiveExpression (value);
		}
		
		public WidgetMap WidgetMap {
			get { return map; }
		}
		
		public void EndGeneration ()
		{
			foreach (Stetic.Wrapper.Widget w in generatedWrappers) {
				string v = (string) vars [w.Wrapped.Name];
				w.GeneratePostBuildCode (this, v);
			}
		}
		
		public void Reset ()
		{
			vars.Clear ();
			widgets.Clear ();
			generatedWrappers.Clear ();
			map = new WidgetMap (vars, widgets);
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


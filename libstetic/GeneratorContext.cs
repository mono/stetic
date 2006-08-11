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
			varDec.InitExpression = widget.GenerateObjectCreation (this);
			statements.Add (varDec);
			GenerateBuildCode (widget, varName);
			return varName;
		}
		
		public virtual void GenerateBuildCode (ObjectWrapper wrapper, string varName)
		{
			vars [wrapper] = varName;
			widgets [varName] = wrapper.Wrapped;
			wrapper.GenerateBuildCode (this, varName);
			generatedWrappers.Add (wrapper);
		}
		
		public virtual void GenerateBuildCode (Wrapper.ActionGroup agroup, string varName)
		{
			vars [agroup] = varName;
			widgets [varName] = agroup;
			agroup.GenerateBuildCode (this, varName);
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
			
			if (value is Wrapper.ActionGroup) {
				return new CodeMethodInvokeExpression (
					new CodeMethodReferenceExpression (
						new CodeTypeReferenceExpression (CodeNamespace.Name + ".ActionGroups"),
						"GetActionGroup"
					),
					new CodePrimitiveExpression (((Wrapper.ActionGroup)value).Name)
				);
			}
			
			if (value is Array) {
				ArrayList list = new ArrayList ();
				foreach (object val in (Array)value)
					list.Add (GenerateValue (val, val != null ? val.GetType() : null));
				return new CodeArrayCreateExpression (value.GetType().GetElementType(), (CodeExpression[]) list.ToArray(typeof(CodeExpression)));
			}
			
			if (value is DateTime) {
				return new CodeObjectCreateExpression (
					typeof(DateTime),
					new CodePrimitiveExpression (((DateTime)value).Ticks)
				);
			}
			
			if (value is TimeSpan) {
				return new CodeObjectCreateExpression (
					typeof(TimeSpan),
					new CodePrimitiveExpression (((TimeSpan)value).Ticks)
				);
			}
			
			return new CodePrimitiveExpression (value);
		}
		
		public WidgetMap WidgetMap {
			get { return map; }
		}
		
		public void EndGeneration ()
		{
			foreach (ObjectWrapper w in generatedWrappers) {
				string v = (string) vars [w];
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
		
		public string GetWidgetId (ObjectWrapper wrapper)
		{
			return (string) vars [wrapper];
		}
		
		public string GetWidgetId (object wrapped)
		{
			ObjectWrapper w = ObjectWrapper.Lookup (wrapped);
			if (w != null)
				return GetWidgetId (w);
			else
				return null;
		}
		
		public ObjectWrapper GetWidgetFromId (string widgetId)
		{
			return (ObjectWrapper) widgets [widgetId];
		}
	}
}


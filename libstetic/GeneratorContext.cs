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
		ArrayList generatedWrappers = new ArrayList ();
		WidgetMap map;
		CodeStatementCollection statements;
		GenerationOptions options;
		
		public GeneratorContext (CodeNamespace cns, string idPrefix, CodeStatementCollection statements, GenerationOptions options)
		{
			this.cns = cns;
			this.idPrefix = idPrefix;
			this.statements = statements;
			this.options = options;
			map = new WidgetMap (vars);
		}
		
		public CodeNamespace GlobalCodeNamespace {
			get { return cns; }
		}
		
		public CodeStatementCollection Statements {
			get { return statements; }
		}
		
		public GenerationOptions Options {
			get { return options; }
		}
		
		public string NewId ()
		{
			return idPrefix + (++n);
		}
		
		public CodeExpression GenerateNewInstanceCode (Wrapper.Widget widget)
		{
			CodeExpression var = GenerateInstanceExpression (widget, widget.GenerateObjectCreation (this));
			GenerateBuildCode (widget, var);
			return var;
		}
		
		public virtual CodeExpression GenerateInstanceExpression (ObjectWrapper wrapper, CodeExpression newObject)
		{
			string varName = NewId ();
			CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (wrapper.WrappedTypeName, varName);
			varDec.InitExpression = newObject;
			statements.Add (varDec);
			return new CodeVariableReferenceExpression (varName);
		}
		
		public virtual void GenerateCreationCode (ObjectWrapper wrapper, CodeExpression varExp)
		{
			wrapper.GenerateInitCode (this, varExp);
			GenerateBuildCode (wrapper, varExp);
		}
		
		public virtual void GenerateBuildCode (ObjectWrapper wrapper, CodeExpression var)
		{
			vars [wrapper] = var;
			wrapper.GenerateBuildCode (this, var);
			generatedWrappers.Add (wrapper);
		}
		
		public virtual void GenerateCreationCode (Wrapper.ActionGroup agroup, CodeExpression var)
		{
			vars [agroup] = var;
			agroup.GenerateBuildCode (this, var);
		}
		
		public CodeExpression GenerateValue (object value, Type type)
		{
			return GenerateValue (value, type, false);
		}
		
		public CodeExpression GenerateValue (object value, Type type, bool translatable)
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
						new CodeTypeReferenceExpression (GlobalCodeNamespace.Name + ".ActionGroups"),
						"GetActionGroup"
					),
					new CodePrimitiveExpression (((Wrapper.ActionGroup)value).Name)
				);
			}
			
			if (value is Array) {
				ArrayList list = new ArrayList ();
				foreach (object val in (Array)value)
					list.Add (GenerateValue (val, val != null ? val.GetType() : null, translatable));
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
			
			if (value is string && translatable && options.UseGettext) {
				return new CodeMethodInvokeExpression (
					new CodeTypeReferenceExpression (typeof(Mono.Unix.Catalog)),
					"GetString",
					new CodePrimitiveExpression (value)
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
				CodeExpression var = (CodeExpression) vars [w];
				w.GeneratePostBuildCode (this, var);
			}
		}
		
		public void Reset ()
		{
			vars.Clear ();
			generatedWrappers.Clear ();
			map = new WidgetMap (vars);
			n = 0;
		}
	}

	public class WidgetMap
	{
		Hashtable vars;
		
		internal WidgetMap (Hashtable vars)
		{
			this.vars = vars;
		}
		
		public CodeExpression GetWidgetExp (ObjectWrapper wrapper)
		{
			return (CodeExpression) vars [wrapper];
		}
		
		public CodeExpression GetWidgetExp (object wrapped)
		{
			ObjectWrapper w = ObjectWrapper.Lookup (wrapped);
			if (w != null)
				return GetWidgetExp (w);
			else
				return null;
		}
	}
	
	[Serializable]
	public class GenerationOptions
	{
		bool useGettext;
		bool partialClasses;
		bool generateEmptyBuildMethod;
		bool generateSingleFile = true;
		string path;
		string globalNamespace = "Stetic";
		
		public bool UseGettext {
			get { return useGettext; }
			set { useGettext = value; }
		}
		
		public bool UsePartialClasses {
			get { return partialClasses; }
			set { partialClasses = value; }
		}
		
		public string Path {
			get { return path; }
			set { path = value; }
		}
		
		public bool GenerateEmptyBuildMethod {
			get { return generateEmptyBuildMethod; }
			set { generateEmptyBuildMethod = value; }
		}
		
		public bool GenerateSingleFile {
			get { return generateSingleFile; }
			set { generateSingleFile = value; }
		}
		
		public string GlobalNamespace {
			get { return globalNamespace; }
			set { globalNamespace = value; }
		}
	}
}


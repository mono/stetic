
using System;
using System.CodeDom;
using System.Xml;
using System.Collections;
using Stetic.Editor;

namespace Stetic.Wrapper
{
	public class ActionToolbarWrapper: Container
	{
		ActionTree actionTree;
		XmlElement toolbarInfo;
		ToolbarStyle toolbarStyle;
		static Gtk.ToolbarStyle defaultStyle;
		static bool gotDefault;
		
		public enum ToolbarStyle {
			Icons,
			Text,
			Both,
			BothHoriz,
			Default
		}
	
		public ActionToolbarWrapper()
		{
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			DisposeTree ();
		}

		public static new Gtk.Toolbar CreateInstance ()
		{
			ActionToolbar t = new ActionToolbar ();
			return t;
		}
		
		ActionToolbar toolbar {
			get { return (ActionToolbar) Wrapped; }
		}
		
		protected override bool AllowPlaceholders {
			get { return false; }
		}
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			CreateTree ();
		}
		
		public override bool HExpandable {
			get {
				return toolbar.Orientation == Gtk.Orientation.Horizontal;
			}
		}

		public override bool VExpandable {
			get {
				return toolbar.Orientation == Gtk.Orientation.Vertical;
			}
		}

		public Gtk.Orientation Orientation {
			get {
				return toolbar.Orientation;
			}
			set {
				toolbar.Orientation = value;
				EmitContentsChanged ();
			}
		}
		
		public Gtk.IconSize IconSize {
			get { return toolbar.IconSize; }
			set { toolbar.IconSize = value; EmitNotify ("IconSize"); }
		}
		
		public ToolbarStyle ButtonStyle {
			get { return toolbarStyle; }
			set {
				toolbarStyle = value;
				if (value == ToolbarStyle.Default) {
					if (!gotDefault) {
						// Is there a better way of getting the default?
						Gtk.Window d = new Gtk.Window ("");
						Gtk.Toolbar t = new Gtk.Toolbar ();
						d.Add (t);
						defaultStyle = t.ToolbarStyle;
						d.Destroy ();
					}
					toolbar.ToolbarStyle = defaultStyle;
				} else {
					toolbar.ToolbarStyle = (Gtk.ToolbarStyle) ((int)value);
				}
				EmitNotify ("ButtonStyle");
			}
		}
		
		internal protected override void OnSelected ()
		{
			Loading = true;
			toolbar.ShowInsertPlaceholder = true;
			Loading = false;
		}
		
		internal protected override void OnUnselected ()
		{
			base.OnUnselected ();
			Loading = true;
			toolbar.ShowInsertPlaceholder = false;
			toolbar.Unselect ();
			Loading = false;
		}
		
		public override XmlElement Write (ObjectWriter writer)
		{
			XmlElement elem = base.Write (writer);
			if (writer.Format == FileFormat.Native) {
				// The style is already stored in ButtonStyle
				GladeUtils.ExtractProperty (elem, "ToolbarStyle", "");
				if (toolbarInfo != null)
					elem.AppendChild (writer.XmlDocument.ImportNode (toolbarInfo, true));
				else
					elem.AppendChild (actionTree.Write (writer.XmlDocument, writer.Format));
			}
			return elem;
		}
		
		public override void Read (ObjectReader reader, XmlElement elem)
		{
			base.Read (reader, elem);
			toolbarInfo = elem ["node"];
		}
		
		protected override void OnNameChanged (WidgetNameChangedArgs args)
		{
			base.OnNameChanged (args);
			if (actionTree != null)
				actionTree.Name = Wrapped.Name;
		}
		
		internal protected override CodeExpression GenerateObjectCreation (GeneratorContext ctx)
		{
			BuildTree ();
			actionTree.Type = Gtk.UIManagerItemType.Toolbar;
			actionTree.Name = Wrapped.Name;
			
			CodeExpression exp = GenerateUiManagerElement (ctx, actionTree);
			if (exp != null)
				return new CodeCastExpression (typeof(Gtk.Toolbar),	exp);
			else
				return base.GenerateObjectCreation (ctx);
		}

		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			base.GenerateBuildCode (ctx, varName);
		}
		
		protected override void GeneratePropertySet (GeneratorContext ctx, CodeVariableReferenceExpression var, PropertyDescriptor prop)
		{
			if (toolbarStyle == ToolbarStyle.Default && prop.Name == "ToolbarStyle")
				return;
			else
				base.GeneratePropertySet (ctx, var, prop);
		}
		
		internal protected override void OnDesignerAttach (IDesignArea designer)
		{
			base.OnDesignerAttach (designer);
			BuildTree ();
			
			Loading = true;
			toolbar.FillMenu (actionTree);
			Loading = false;
			
			if (LocalActionGroups.Count == 0)
				LocalActionGroups.Add (new ActionGroup ("Default"));
		}
		
		protected override void EmitNotify (string propertyName)
		{
			base.EmitNotify (propertyName);
			toolbar.FillMenu (actionTree);
		}
		
		void BuildTree ()
		{
			if (toolbarInfo != null) {
				DisposeTree ();
				CreateTree ();
				actionTree.Read (this, toolbarInfo);
				toolbarInfo = null;
			}
		}
		
		void CreateTree ()
		{
			actionTree = new ActionTree ();
			actionTree.Name = Wrapped.Name;
			actionTree.Type = Gtk.UIManagerItemType.Menubar;
			actionTree.Changed += OnTreeChanged;
		}
		
		void DisposeTree ()
		{
			if (actionTree != null) {
				actionTree.Dispose ();
				actionTree.Changed -= OnTreeChanged;
				actionTree = null;
			}
		}
		
		void OnTreeChanged (object s, EventArgs a)
		{
			NotifyChanged ();
		}
	}
}

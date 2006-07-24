
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
		
		public Gtk.ToolbarStyle ToolbarStyle {
			get { return toolbar.ToolbarStyle; }
			set { toolbar.ToolbarStyle = value; EmitNotify ("ToolbarStyle"); }
		}
		
		internal protected override void OnSelected ()
		{
			toolbar.ShowInsertPlaceholder = true;
		}
		
		internal protected override void OnUnselected ()
		{
			base.OnUnselected ();
			toolbar.ShowInsertPlaceholder = false;
			toolbar.Unselect ();
		}
		
		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			if (toolbarInfo != null)
				elem.AppendChild (doc.ImportNode (toolbarInfo, true));
			else
				elem.AppendChild (actionTree.Write (doc, format));
			return elem;
		}
		
		public override void Read (XmlElement elem, FileFormat format)
		{
			base.Read (elem, format);
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
		
		internal protected override void OnDesignerAttach (IDesignArea designer)
		{
			base.OnDesignerAttach (designer);
			BuildTree ();
			toolbar.FillMenu (actionTree);
			
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
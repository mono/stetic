
using System;
using System.CodeDom;
using System.Xml;
using System.Collections;
using Stetic.Editor;

namespace Stetic.Wrapper
{
	public class MenuBar: Container
	{
		ActionTree actionTree;
		XmlElement menuInfo;
		
		public MenuBar()
		{
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			DisposeTree ();
		}
		
		public static new Gtk.MenuBar CreateInstance ()
		{
			return new ActionMenuBar ();
		}
		
		protected override bool AllowPlaceholders {
			get { return false; }
		}
		
		ActionMenuBar menu {
			get { return (ActionMenuBar) Wrapped; }
		}
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			CreateTree ();
		}
		
		internal protected override void OnSelected ()
		{
			Loading = true;
			menu.ShowInsertPlaceholder = true;
			Loading = false;
		}
		
		internal protected override void OnUnselected ()
		{
			base.OnUnselected ();
			Loading = true;
			menu.ShowInsertPlaceholder = false;
			menu.Unselect ();
			Loading = false;
		}
		
		public override XmlElement Write (ObjectWriter writer)
		{
			XmlElement elem = base.Write (writer);
			if (menuInfo != null)
				elem.AppendChild (writer.XmlDocument.ImportNode (menuInfo, true));
			else
				elem.AppendChild (actionTree.Write (writer.XmlDocument, writer.Format));
			return elem;
		}
		
		public override void Read (ObjectReader reader, XmlElement elem)
		{
			base.Read (reader, elem);
			menuInfo = elem ["node"];
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
			CodeExpression exp = GenerateUiManagerElement (ctx, actionTree);
			if (exp != null)
				return new CodeCastExpression (typeof(Gtk.MenuBar),	exp);
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
			Loading = true;
			menu.FillMenu (actionTree);
			Loading = false;
			
			if (LocalActionGroups.Count == 0)
				LocalActionGroups.Add (new ActionGroup ("Default"));
		}
		
		void BuildTree ()
		{
			if (menuInfo != null) {
				DisposeTree ();
				CreateTree ();
				actionTree.Read (this, menuInfo);
				menuInfo = null;
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
	
	class CustomMenuBarItem: Gtk.MenuItem
	{
		public ActionMenuItem ActionMenuItem;
		public ActionTreeNode Node;
	}
		
	public class ActionPaletteItem: Gtk.HBox
	{
		ActionTreeNode node;
		
		public ActionPaletteItem (Gtk.UIManagerItemType type, string name, Action action) 
		: this (new ActionTreeNode (type, name, action))
		{
		}
		
		public ActionPaletteItem (ActionTreeNode node)
		{
			this.node = node;
			Spacing = 3;
			if (node.Type == Gtk.UIManagerItemType.Menu) {
				PackStart (new Gtk.Label ("Menu"), true, true, 0);
			} else if (node.Action != null && node.Action.GtkAction != null) {
				if (node.Action.GtkAction.StockId != null)
					PackStart (node.Action.CreateIcon (Gtk.IconSize.Menu), true, true, 0);
				PackStart (new Gtk.Label (node.Action.GtkAction.Label), true, true, 0);
			} else if (node.Type == Gtk.UIManagerItemType.Separator) {
				PackStart (new Gtk.Label ("Separator"), true, true, 0);
			} else {
				PackStart (new Gtk.Label ("Empty Action"), true, true, 0);
			}
			ShowAll ();
		}
		
		public ActionTreeNode Node {
			get { return node; }
		}
	}
}


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
		
		public static new Gtk.MenuBar CreateInstance ()
		{
			return new ActionMenuBar ();
		}
		
		ActionMenuBar menu {
			get { return (ActionMenuBar) Wrapped; }
		}
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			actionTree = new ActionTree ();
		}
		
		internal protected override void OnSelected ()
		{
			menu.ShowInsertPlaceholder = true;
		}
		
		internal protected override void OnUnselected ()
		{
			base.OnUnselected ();
			menu.ShowInsertPlaceholder = false;
			menu.Unselect ();
		}
		
		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			if (menuInfo != null)
				elem.AppendChild (doc.ImportNode (menuInfo, true));
			else
				elem.AppendChild (actionTree.Write (doc, format));
			return elem;
		}
		
		public override void Read (XmlElement elem, FileFormat format)
		{
			base.Read (elem, format);
			menuInfo = elem ["node"];
		}
		
		internal protected override CodeExpression GenerateObjectCreation (GeneratorContext ctx)
		{
			BuildTree ();
			actionTree.Type = Gtk.UIManagerItemType.Menubar;
			actionTree.Name = Wrapped.Name;
			
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
			menu.FillMenu (actionTree);
			
			if (LocalActionGroups.Count == 0)
				LocalActionGroups.Add (new ActionGroup ("Default"));
		}
		
		void BuildTree ()
		{
			if (menuInfo != null) {
				actionTree = new ActionTree ();
				actionTree.Read (this, menuInfo);
				menuInfo = null;
			}
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

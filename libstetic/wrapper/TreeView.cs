using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Tree View", "treeview.png", typeof (Gtk.TreeView), ObjectWrapperType.Widget)]
	public class TreeView : Stetic.Wrapper.Container {

		public static ItemGroup TreeViewProperties;
		public static ItemGroup TreeViewExtraProperties;

		static TreeView () {
			TreeViewProperties = new ItemGroup ("Tree View Properties",
							    typeof (Stetic.Wrapper.TreeView),
							    typeof (Gtk.TreeView),
							    "EnableSearch",
							    "FixedHeightMode",
							    "HeadersVisible",
							    "Reorderable",
							    "RulesHint",
							    "SearchColumn",
							    "Model");
			RegisterWrapper (typeof (Stetic.Wrapper.TreeView),
					 TreeViewProperties,
					 Widget.CommonWidgetProperties);
		}

		public TreeView (IStetic stetic) : this (stetic, new Gtk.TreeView (), false) {}


		public TreeView (IStetic stetic, Gtk.TreeView treeview, bool initialized) : base (stetic, treeview, initialized)
		{
		}
	}
}

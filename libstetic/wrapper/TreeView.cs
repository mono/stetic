using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Tree View", "treeview.png", ObjectWrapperType.Widget)]
	public class TreeView : Container {

		public static new Type WrappedType = typeof (Gtk.TreeView);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Tree View Properties",
				      "EnableSearch",
				      "FixedHeightMode",
				      "HeadersVisible",
				      "Reorderable",
				      "RulesHint",
				      "SearchColumn",
				      "Model");
		}
	}
}

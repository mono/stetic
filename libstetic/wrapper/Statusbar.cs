using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Statusbar", "statusbar.png", ObjectWrapperType.Widget)]
	public class Statusbar : Widget {

		public static new Type WrappedType = typeof (Gtk.Statusbar);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Status Bar Properties",
				      "HasResizeGrip");
		}

		public override bool HExpandable { get { return true; } }
	}
}

using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Dialog Box", "dialog.png", ObjectWrapperType.Window)]
	public class Dialog : Window {


		public static new Type WrappedType = typeof (Gtk.Dialog);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Dialog Properties",
				      "Title",
				      "Icon",
				      "WindowPosition",
				      "Modal",
				      "BorderWidth");
			AddItemGroup (type, "Miscellaneous Dialog Properties",
				      "HasSeparator",
				      "AcceptFocus",
				      "Decorated",
				      "DestroyWithParent",
				      "Gravity",
				      "Role",
				      "SkipPagerHint",
				      "SkipTaskbarHint");
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				WidgetSite site = CreateWidgetSite ();
				Gtk.Requisition req;
				req.Width = req.Height = 200;
				site.EmptySize = req;
				((Gtk.Dialog)Wrapped).VBox.Add (site);
			}
		}
	}
}

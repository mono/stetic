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

		public override void Wrap (object obj, bool initialized)
		{
			WidgetSite site;

			base.Wrap (obj, initialized);
			Gtk.Dialog dialog = (Gtk.Dialog)Wrapped;

			site = CreateWidgetSite ();
			site.InternalChildId = "vbox";
			dialog.VBox.Reparent (site);
			dialog.Add (site);

			site = CreateWidgetSite ();
			site.InternalChildId = "action_area";
			dialog.ActionArea.Reparent (site);
			dialog.VBox.PackEnd (site, false, true, 0);

			if (!initialized) {
				site = CreateWidgetSite ();
				Gtk.Requisition req;
				req.Width = req.Height = 200;
				site.EmptySize = req;
				dialog.VBox.Add (site);
			}
		}
	}
}

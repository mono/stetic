using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("MenuItem", "menuitem.png", ObjectWrapperType.Widget)]
	public class MenuItem : Container {

		public static new Type WrappedType = typeof (Gtk.MenuItem);

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			string label = GladeUtils.ExtractProperty ("label", props);
			string use_underline = GladeUtils.ExtractProperty ("use_underline", props);
			base.GladeImport (className, id, props);

			if (label != null) {
				Gtk.MenuItem item = (Gtk.MenuItem)Wrapped;
				Gtk.Label item_label = new Gtk.Label (label);
				item_label.UseUnderline = (use_underline == "True");
				item_label.Show ();
				item.Add (item_label);
			}
		}
	}
}

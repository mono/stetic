using System;
using System.Xml;

namespace Stetic.Wrapper {

	public class ButtonBox : Box {

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			Widget wrapper = base.GladeImportChild (child_elem);
			Button button = wrapper as Stetic.Wrapper.Button;
			if (button != null && button.ResponseId == (int)Gtk.ResponseType.Help) {
				Gtk.ButtonBox.ButtonBoxChild bbc = ((Gtk.Container)Wrapped)[((Gtk.Widget)wrapper.Wrapped)] as Gtk.ButtonBox.ButtonBoxChild;
				bbc.Secondary = true;
			}

			return wrapper;
		}
	}
}

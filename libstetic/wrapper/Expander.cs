using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Expander : Bin {

		public static new Gtk.Expander CreateInstance ()
		{
			return new Gtk.Expander ("");
		}

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			if ((string)GladeUtils.GetChildProperty (child_elem, "type", "") == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, child_elem["widget"]);
				expander.LabelWidget = (Gtk.Widget)wrapper.Wrapped;
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (child_elem);
		}

		public override XmlElement GladeExportChild (Widget wrapper, XmlDocument doc)
		{
			XmlElement child_elem = base.GladeExportChild (wrapper, doc);
			if (wrapper.Wrapped == expander.LabelWidget)
				GladeUtils.SetChildProperty (child_elem, "type", "label_item");
			return child_elem;
		}

		Gtk.Expander expander {
			get {
				return (Gtk.Expander)Wrapped;
			}
		}
	}
}

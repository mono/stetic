using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Expander : Bin {

		public static new Gtk.Expander CreateInstance ()
		{
			return new Gtk.Expander ("");
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			if (childprops.Count == 1 && ((string)childprops["type"]) == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
				expander.LabelWidget = (Gtk.Widget)wrapper.Wrapped;
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (className, id, props, childprops);
		}

		public override void GladeExportChild (Widget wrapper, out string className,
						       out string internalId, out string id,
						       out Hashtable props,
						       out Hashtable childprops)
		{
			base.GladeExportChild (wrapper, out className, out internalId,
					       out id, out props, out childprops);
			if (wrapper.Wrapped == expander.LabelWidget) {
				childprops = new Hashtable ();
				childprops["type"] = "label_item";
			}
		}

		Gtk.Expander expander {
			get {
				return (Gtk.Expander)Wrapped;
			}
		}
	}
}

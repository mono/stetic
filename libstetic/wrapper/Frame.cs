using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", ObjectWrapperType.Container)]
	public class Frame : Bin {

		public static new Type WrappedType = typeof (Gtk.Frame);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Frame Properties",
				      "Shadow",
				      "ShadowType",
				      "Label",
				      "LabelXalign",
				      "LabelYalign",
				      "BorderWidth");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized)
				frame.Label = frame.Name;
		}

		Gtk.Frame frame {
			get {
				return (Gtk.Frame)Wrapped;
			}
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			if (childprops.Count == 1 && ((string)childprops["type"]) == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
				frame.LabelWidget = (Gtk.Widget)wrapper.Wrapped;
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
			if (wrapper.Wrapped == frame.LabelWidget) {
				childprops = new Hashtable ();
				childprops["type"] = "label_item";
			}
		}
	}
}

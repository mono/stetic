using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", ObjectWrapperType.Container)]
	public class Frame : Bin {

		public static new Type WrappedType = typeof (Gtk.Frame);

		static new void Register (Type type)
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
			if (!initialized) {
				Gtk.Frame frame = (Gtk.Frame)Wrapped;
				frame.Label = frame.Name;
			}
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			if (childprops.Count == 1 && ((string)childprops["type"]) == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
				WidgetSite site = CreateWidgetSite ((Gtk.Widget)wrapper.Wrapped);

				Gtk.Frame frame = (Gtk.Frame)Wrapped;
				frame.LabelWidget = site;
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (className, id, props, childprops);
		}
	}
}

using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Expander", "expander.png", ObjectWrapperType.Container)]
	public class Expander : Bin {

		public static new Type WrappedType = typeof (Gtk.Expander);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Expander Properties",
				      "Expanded",
				      "Label",
				      "UseMarkup",
				      "UseUnderline",
				      "Spacing",
				      "BorderWidth");
		}

		public static new Gtk.Expander CreateInstance ()
		{
			return new Gtk.Expander ("");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized)
				expander.Label = expander.Name;
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			if (childprops.Count == 1 && ((string)childprops["type"]) == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
				WidgetSite site = CreateWidgetSite ((Gtk.Widget)wrapper.Wrapped);

				expander.LabelWidget = site;
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (className, id, props, childprops);
		}

		Gtk.Expander expander {
			get {
				return (Gtk.Expander)Wrapped;
			}
		}
	}
}

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

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Frame frame = (Gtk.Frame)Wrapped;
				frame.Label = frame.Name;
			}
		}

		public override Widget GladeImportChild (string className, string id,
							 ArrayList propNames, ArrayList propVals,
							 ArrayList packingNames, ArrayList packingVals)
		{
			if (packingNames.Count == 1 &&
			    (string)packingNames[0] == "type" &&
			    (string)packingVals[0] == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, propNames, propVals);
				WidgetSite site = CreateWidgetSite ();
				site.Add ((Gtk.Widget)wrapper.Wrapped);

				Gtk.Frame frame = (Gtk.Frame)Wrapped;
				frame.LabelWidget = site;
				return (Widget)wrapper;
			} else {
				return base.GladeImportChild (className, id,
							      propNames, propVals,
							      packingNames, packingVals);
			}
		}
	}
}

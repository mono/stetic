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

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			if (!initialized)
				expander.Label = expander.Name;

			expander.Activated += delegate (object obj, EventArgs args) {
				EmitContentsChanged ();
			};
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

				expander.LabelWidget = site;
				return (Widget)wrapper;
			} else {
				return base.GladeImportChild (className, id,
							      propNames, propVals,
							      packingNames, packingVals);
			}
		}

		Gtk.Expander expander {
			get {
				return (Gtk.Expander)Wrapped;
			}
		}

		public override bool HExpandable {
			get {
				return expander.Expanded && site.HExpandable;
			}
		}
		public override bool VExpandable {
			get {
				return expander.Expanded && site.VExpandable;
			}
		}
	}
}

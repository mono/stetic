using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Expander", "expander.png", ObjectWrapperType.Container)]
	public class Expander : Bin {

		public static ItemGroup ExpanderProperties;

		static Expander () {
			ExpanderProperties = new ItemGroup ("Expander Properties",
							    typeof (Gtk.Expander),
							    "Expanded",
							    "Label",
							    "UseMarkup",
							    "UseUnderline",
							    "Spacing",
							    "BorderWidth");
			RegisterItems (typeof (Stetic.Wrapper.Expander),
				       ExpanderProperties,
				       Widget.CommonWidgetProperties);
		}

		public Expander (IStetic stetic) : this (stetic, new Gtk.Expander ("")) {}

		public Expander (IStetic stetic, Gtk.Expander expander) : base (stetic, expander)
		{
			expander.Label = expander.Name;
			expander.Activated += delegate (object obj, EventArgs args) {
				EmitContentsChanged ();
			};
		}

		public override bool HExpandable {
			get {
				return ((Gtk.Expander)Wrapped).Expanded && site.HExpandable;
			}
		}
		public override bool VExpandable {
			get {
				return ((Gtk.Expander)Wrapped).Expanded && site.VExpandable;
			}
		}
	}
}

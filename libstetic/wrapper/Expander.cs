using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Expander", "expander.png", typeof (Gtk.Expander), ObjectWrapperType.Container)]
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
			RegisterWrapper (typeof (Stetic.Wrapper.Expander),
					 ExpanderProperties,
					 Widget.CommonWidgetProperties);
		}

		public Expander (IStetic stetic) : this (stetic, new Gtk.Expander (""), false) {}

		public Expander (IStetic stetic, Gtk.Expander expander, bool initialized) : base (stetic, expander, initialized)
		{
			if (!initialized) {
				expander.Label = expander.Name;
			}
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

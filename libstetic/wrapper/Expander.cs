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
			groups = new ItemGroup[] {
				ExpanderProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public Expander (IStetic stetic) : this (stetic, new Gtk.Expander ("Expander")) {}

		public Expander (IStetic stetic, Gtk.Expander expander) : base (stetic, expander)
		{
			expander.Activated += delegate (object obj, EventArgs args) {
				EmitContentsChanged ();
			};
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }

		private Gtk.Expander expander {
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

using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Expander", "expander.png", ObjectWrapperType.Container)]
	public class Expander : Bin {

		public static PropertyGroup ExpanderProperties;

		static Expander () {
			ExpanderProperties = new PropertyGroup ("Expander Properties",
								typeof (Gtk.Expander),
								"Expanded",
								"Label",
								"UseMarkup",
								"UseUnderline",
								"Spacing",
								"BorderWidth");
			groups = new PropertyGroup[] {
				ExpanderProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Expander (IStetic stetic) : this (stetic, new Gtk.Expander ("Expander")) {}

		public Expander (IStetic stetic, Gtk.Expander expander) : base (stetic, expander)
		{
			expander.Activated += delegate (object obj, EventArgs args) {
				EmitContentsChanged ();
			};
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

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

using System;

namespace Stetic.Wrapper {

	public abstract class Scrollbar : Range {
		static Scrollbar () {
			groups = new PropertyGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		protected Scrollbar (IStetic stetic, Gtk.Scrollbar scrollbar) : base (stetic, scrollbar) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}

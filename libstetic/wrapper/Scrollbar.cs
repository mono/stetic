using System;

namespace Stetic.Wrapper {

	public abstract class Scrollbar : Range {
		static Scrollbar () {
			groups = new ItemGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		protected Scrollbar (IStetic stetic, Gtk.Scrollbar scrollbar) : base (stetic, scrollbar) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}

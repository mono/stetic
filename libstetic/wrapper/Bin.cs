using System;

namespace Stetic.Wrapper {

	public abstract class Bin : Stetic.Wrapper.Container {

		protected WidgetSite site;

		protected Bin (IStetic stetic, Gtk.Bin bin) : base (stetic, bin)
		{
			site = CreateWidgetSite ();
			bin.Add (site);
		}

		public override bool HExpandable { get { return site.HExpandable; } }
		public override bool VExpandable { get { return site.VExpandable; } }
	}
}


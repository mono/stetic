using System;

namespace Stetic.Wrapper {

	public abstract class Bin : Stetic.Wrapper.Container {

		static Bin ()
		{
			RegisterWrapper (typeof (Stetic.Wrapper.Bin),
					 new ItemGroup[0]);
		}

		protected WidgetSite site;

		protected Bin (IStetic stetic, Gtk.Bin bin) : base (stetic, bin)
		{
			if (bin.Child == null) {
				site = CreateWidgetSite ();
				bin.Add (site);
			}
		}

		public override bool HExpandable { get { return site.HExpandable; } }
		public override bool VExpandable { get { return site.VExpandable; } }
	}
}


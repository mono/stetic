using System;

namespace Stetic.Wrapper {

	public abstract class Bin : Stetic.Wrapper.Container {

		static Bin ()
		{
			RegisterWrapper (typeof (Stetic.Wrapper.Bin),
					 new ItemGroup[0]);
		}

		protected WidgetSite site;

		protected WidgetSite Site {
			get {
				if (site == null)
					site = ((Gtk.Bin)Wrapped).Child as WidgetSite;
				return site;
			}
		}
		
		protected Bin (IStetic stetic, Gtk.Bin bin, bool initialized) : base (stetic, bin, initialized)
		{
			if (!initialized && bin.Child == null) {
				site = CreateWidgetSite ();
				bin.Add (site);
			}
		}

		public override bool HExpandable {
			get { 
				if (Site != null)
					return Site.HExpandable;
				return false;
			}
		}
		public override bool VExpandable {
			get {
				if (Site != null)
					return Site.VExpandable;
				return false;
			}
		}
	}
}


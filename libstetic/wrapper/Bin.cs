using System;

namespace Stetic.Wrapper {

	public abstract class Bin : Container {

		public static new Type WrappedType = typeof (Gtk.Bin);

		protected WidgetSite site;
		protected WidgetSite Site {
			get {
				if (site == null)
					site = ((Gtk.Bin)Wrapped).Child as WidgetSite;
				return site;
			}
		}
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.Bin bin = (Gtk.Bin)obj;
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


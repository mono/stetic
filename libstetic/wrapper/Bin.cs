using System;

namespace Stetic.Wrapper {

	public abstract class Bin : Container {

		public static new Type WrappedType = typeof (Gtk.Bin);

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized && bin.Child == null)
				bin.Add (CreatePlaceholder ());
		}

		Gtk.Bin bin {
			get {
				return (Gtk.Bin)Wrapped;
			}
		}

		public override bool HExpandable {
			get { 
				WidgetBox child = bin.Child as WidgetBox;
				if (child != null)
					return child.HExpandable;
				return false;
			}
		}
		public override bool VExpandable {
			get {
				WidgetBox child = bin.Child as WidgetBox;
				if (child != null)
					return child.VExpandable;
				return false;
			}
		}
	}
}


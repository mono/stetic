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
				return ChildHExpandable (bin.Child);
			}
		}
		public override bool VExpandable {
			get {
				return ChildVExpandable (bin.Child);
			}
		}
	}
}


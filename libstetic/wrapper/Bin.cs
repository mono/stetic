using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Bin : Container {

		public static new Type WrappedType = typeof (Gtk.Bin);

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized && bin.Child == null)
				AddPlaceholder ();
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			base.GladeImport (className, id, props);
			stetic.GladeImportComplete += delegate () {
				if (bin.Child == null)
					AddPlaceholder ();
			};
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


using System;
using System.Xml;

namespace Stetic.Wrapper {

	public class Bin : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized && bin.Child == null)
				AddPlaceholder ();
		}

		public override void GladeImport (XmlElement elem)
		{
			base.GladeImport (elem);
			if (elem["child"] == null)
				AddPlaceholder ();
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


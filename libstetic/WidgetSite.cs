using Gtk;
using Gdk;
using System;
using System.Collections;

namespace Stetic {

	public class WidgetSite : WidgetBox {

		public WidgetSite (Widget child)
		{
			Add (child);
		}

		public override bool HExpandable {
			get {
				Stetic.Wrapper.Widget child = Stetic.Wrapper.Widget.Lookup (Child);
				return child.HExpandable;
			}
		}

		public override bool VExpandable {
			get {
				Stetic.Wrapper.Widget child = Stetic.Wrapper.Widget.Lookup (Child);
				return child.VExpandable;
			}
		}

		public override string ToString ()
		{
			if (Child.Name == null)
				return "[WidgetSite " + GetHashCode().ToString() + ": " + Child.ToString() + " " + Child.GetHashCode().ToString() + "]";
			else
				return "[WidgetSite " + GetHashCode().ToString() + ": " + Child.ToString() + " '" + Child.Name + "' " + Child.GetHashCode().ToString() + "]";
		}
	}
}

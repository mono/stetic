using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Widget : Stetic.Wrapper.Object {
		public static ItemGroup CommonWidgetProperties;

		static Widget () {
			CommonWidgetProperties = new ItemGroup ("Common Widget Properties",
								typeof (Gtk.Widget),
								"WidthRequest",
								"HeightRequest",
								"Visible",
								"Sensitive",
								"CanDefault",
								"HasDefault",
								"CanFocus",
								"HasFocus",
								"Events",
								"ExtensionEvents");
			RegisterWrapper (typeof (Stetic.Wrapper.Widget),
					 CommonWidgetProperties);
		}

		static Hashtable counters = new Hashtable ();

		
		protected Widget (IStetic stetic, Gtk.Widget widget, bool initialized) : base (stetic, widget)
		{
			if (!(widget is Gtk.Window))
				widget.ShowAll ();

			Type type = GetType ();
			if (!counters.Contains (type))
				counters[type] = 1;

			if (!initialized) {
				widget.Name = type.Name.ToLower () + ((int)counters[type]).ToString ();
			}
			counters[type] = (int)counters[type] + 1;
		}

		public static new Widget Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Widget;
		}

		protected virtual WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = stetic.CreateWidgetSite ();
			site.Show ();
			return site;
		}

		public virtual bool HExpandable { get { return false; } }
		public virtual bool VExpandable { get { return false; } }
	}
}

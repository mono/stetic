using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Widget : Object {

		public static new Type WrappedType = typeof (Gtk.Widget);

		static new void Register (Type type)
		{
			AddItemGroup (type,
				      "Common Widget Properties",
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
		}

		static Hashtable counters = new Hashtable ();

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.Widget widget = (Gtk.Widget)Wrapped;
			if (!(widget is Gtk.Window))
				widget.ShowAll ();

			Type type = GetType ();
			if (!counters.Contains (type))
				counters[type] = 1;

			if (!initialized)
				widget.Name = type.Name.ToLower () + ((int)counters[type]).ToString ();
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

		bool window_visible;
		public bool Visible {
			get {
				if (Wrapped is Gtk.Window)
					return window_visible;
				else
					return ((Gtk.Widget)Wrapped).Visible;
			}
			set {
				if (Wrapped is Gtk.Window)
					window_visible = value;
				else
					((Gtk.Widget)Wrapped).Visible = value;
			}
		}
	}
}

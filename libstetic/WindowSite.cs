using Gtk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class WindowSite : IWidgetSite {
		Gtk.Window contents;

		static Hashtable windowSites = new Hashtable ();

		public WindowSite (Gtk.Window contents)
		{
			this.contents = contents;
			windowSites[contents] = this;
			contents.DeleteEvent += ContentsDeleted;
			contents.SetFocus += ContentsSetFocus;
		}

		void ContentsDeleted (object obj, EventArgs args)
		{
			if (FocusChanged != null) {
				FocusChanged (this, null);
				FocusChanged = null;
			}
			windowSites.Remove (contents);
		}

		public delegate void FocusChangedHandler (WindowSite site, WidgetBox focus);
		public event FocusChangedHandler FocusChanged;

		[ConnectBefore] // otherwise contents.Focus will be the new focus, not the old
		void ContentsSetFocus (object obj, SetFocusArgs args)
		{
			Widget w;

			w = contents.Focus;
			while (w != null && !(w is WidgetBox))
				w = w.Parent;
			WidgetBox oldf = (WidgetBox)w;

			w = args.Focus;
			while (w != null && !(w is WidgetBox))
				w = w.Parent;
			WidgetBox newf = (WidgetBox)w;

			if (oldf == newf)
				return;

			if (oldf != null)
				oldf.UnFocus ();
			if (newf != null)
				newf.Focus ();

			if (FocusChanged != null)
				FocusChanged (this, newf);
		}

		public static IWidgetSite LookupSite (Widget window)
		{
			return windowSites[window] as IWidgetSite;
		}

		public Widget Contents {
			get {
				return contents;
			}
		}

		public IWidgetSite ParentSite {
			get {
				return null;
			}
		}

		public bool Internal {
			get {
				return false;
			}
		}

		public void Delete ()
		{
			contents.Destroy ();
		}

		public void Select ()
		{
			contents.Show ();
			contents.Focus = null;
		}

		public void UnSelect ()
		{
			;
		}
	}
}

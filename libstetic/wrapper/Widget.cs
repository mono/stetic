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

		public override void Wrap (object obj, bool initialized)
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

		protected virtual void GladeImport (string className, string id, Hashtable props)
		{
			GladeUtils.ImportWidget (stetic, this, className, id, props);
		}

		public virtual void GladeExport (out string className, out string id, out Hashtable props)
		{
			GladeUtils.ExportWidget (stetic, this, out className, out id, out props);
			GladeUtils.ExtractProperty ("name", props);
		}

		public static new Widget Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Widget;
		}

		protected virtual WidgetSite CreateWidgetSite (Gtk.Widget w)
		{
			WidgetSite site = stetic.CreateWidgetSite (w);
			site.Show ();
			return site;
		}

		protected virtual Placeholder CreatePlaceholder ()
		{
			Placeholder ph = stetic.CreatePlaceholder ();
			ph.Show ();
			return ph;
		}

		public virtual bool HExpandable { get { return false; } }
		public virtual bool VExpandable { get { return false; } }

		bool window_visible;
		[GladeProperty]
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

		bool hasDefault;
		[GladeProperty]
		public bool HasDefault {
			get {
				return hasDefault;
			}
			set {
				hasDefault = value;

				Gtk.Widget widget = (Gtk.Widget)Wrapped;
				if (widget.Toplevel != null)
					widget.HasDefault = hasDefault;
				else
					widget.HierarchyChanged += HierarchyChanged;
			}
		}

		void HierarchyChanged (object obj, Gtk.HierarchyChangedArgs args)
		{
			Gtk.Widget widget = (Gtk.Widget)Wrapped;

			if (widget.Toplevel != null) {
				widget.HasDefault = hasDefault;
				widget.HierarchyChanged -= HierarchyChanged;
			}
		}
	}
}

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

			if (!(Wrapped is Gtk.Window))
				Wrapped.ShowAll ();

			Type type = GetType ();
			if (!counters.Contains (type))
				counters[type] = 1;

			if (!initialized)
				Wrapped.Name = type.Name.ToLower () + ((int)counters[type]).ToString ();
			counters[type] = (int)counters[type] + 1;
		}

		public new Gtk.Widget Wrapped {
			get {
				return (Gtk.Widget)base.Wrapped;
			}
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

		public Stetic.Wrapper.Container ParentWrapper {
			get {
				Gtk.Widget p = Wrapped.Parent;
				while (p != null && (p is WidgetSite))
					p = p.Parent;
				return Stetic.Wrapper.Container.Lookup (p);
			}
		}

		string internalChildId;
		public string InternalChildId {
			get {
				return internalChildId;
			}
			set {
				internalChildId = value;

				if (Wrapped != null) {
					WidgetSite site = Wrapped.Parent as WidgetSite;
					if (site != null)
						site.Internal = (value != null);
				}
			}
		}

		public virtual void Select ()
		{
			Gtk.Widget w = Wrapped;
			if (!w.CanFocus && w.Parent is WidgetSite)
				w = w.Parent;
			w.GrabFocus ();
		}

		public virtual void Drop (Gtk.Widget widget, object faultId)
		{
			widget.Destroy ();
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
					return Wrapped.Visible;
			}
			set {
				if (Wrapped is Gtk.Window)
					window_visible = value;
				else
					Wrapped.Visible = value;
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

				if (Wrapped.Toplevel != null)
					Wrapped.HasDefault = hasDefault;
				else
					Wrapped.HierarchyChanged += HierarchyChanged;
			}
		}

		void HierarchyChanged (object obj, Gtk.HierarchyChangedArgs args)
		{
			if (Wrapped.Toplevel != null) {
				Wrapped.HasDefault = hasDefault;
				Wrapped.HierarchyChanged -= HierarchyChanged;
			}
		}
	}
}

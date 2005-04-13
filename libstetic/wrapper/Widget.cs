using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Widget : Object {

		public static new Type WrappedType = typeof (Gtk.Widget);

		internal static new void Register (Type type)
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

			Wrapped.PopupMenu += PopupMenu;
			InterceptClicks (Wrapped);
		}

		void InterceptClicks (Gtk.Widget widget)
		{
			widget.Events |= Gdk.EventMask.ButtonPressMask;
			widget.WidgetEvent += WidgetEvent;

			Gtk.Container container = widget as Gtk.Container;
			if (container != null) {
				foreach (Gtk.Widget child in container.AllChildren) {
					if (Lookup (child) == null)
						InterceptClicks (child);
				}
			}
		}

		public new Gtk.Widget Wrapped {
			get {
				return (Gtk.Widget)base.Wrapped;
			}
		}

		public Stetic.Wrapper.Container ParentWrapper {
			get {
				Gtk.Widget parent = Wrapped.Parent;
				Container wrapper = null;
				while (wrapper == null && parent != null) {
					wrapper = Stetic.Wrapper.Container.Lookup (parent);
					parent = parent.Parent;
				}
				return wrapper;
			}
		}

		[GLib.ConnectBefore]
		void WidgetEvent (object obj, Gtk.WidgetEventArgs args)
		{
			if (args.Event.Type != Gdk.EventType.ButtonPress)
				return;

			Gdk.EventButton evb = (Gdk.EventButton)args.Event;
			int x = (int)evb.X, y = (int)evb.Y;
			int erx, ery, wrx, wry;

			// Translate from event window to widget window coords
			args.Event.Window.GetOrigin (out erx, out ery);
			Wrapped.GdkWindow.GetOrigin (out wrx, out wry);
			x += erx - wrx;
			y += ery - wry;

			Widget wrapper = FindWrapper (Wrapped, x, y);
			if (wrapper == null)
				return;

			if (wrapper != stetic.Selection) {
				wrapper.Select ();
				args.RetVal = true;
			}
			if (evb.Button == 3) {
				stetic.PopupContextMenu (wrapper);
				args.RetVal = true;
			}
		}

		Widget FindWrapper (Gtk.Widget top, int x, int y)
		{
			Gtk.Container container = top as Gtk.Container;
			if (container == null)
				return Lookup (top);

			foreach (Gtk.Widget child in container.AllChildren) {
				if (!child.IsDrawable)
					continue;

				Gdk.Rectangle alloc = child.Allocation;
				if (alloc.Contains (x, y)) {
					Widget wrapper;
					if (child.GdkWindow == top.GdkWindow)
						wrapper = FindWrapper (child, x, y);
					else
						wrapper = FindWrapper (child, x - alloc.X, y - alloc.Y);
					if (wrapper != null)
						return wrapper;
				}
			}

			return Lookup (top);
		}

		void PopupMenu (object obj, EventArgs args)
		{
			stetic.PopupContextMenu (this);
		}

		public void Select ()
		{
			if (ParentWrapper != null)
				ParentWrapper.Select (this);
			else if (this is Stetic.Wrapper.Container)
				((Container)this).Select (this);
		}

		public void UnSelect ()
		{
			if (ParentWrapper != null)
				ParentWrapper.UnSelect (this);
			else if (this is Stetic.Wrapper.Container)
				((Container)this).UnSelect (this);
		}

		public void Delete ()
		{
			UnSelect ();
			if (ParentWrapper != null)
				ParentWrapper.Delete (this);
			else
				Wrapped.Destroy ();
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

		string internalChildId;
		public string InternalChildId {
			get {
				return internalChildId;
			}
			set {
				internalChildId = value;
			}
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

				if (Wrapped.Toplevel != null && Wrapped.Toplevel.IsTopLevel)
					Wrapped.HasDefault = hasDefault;
				else
					Wrapped.HierarchyChanged += HierarchyChanged;
			}
		}

		void HierarchyChanged (object obj, Gtk.HierarchyChangedArgs args)
		{
			if (Wrapped.Toplevel != null && Wrapped.Toplevel.IsTopLevel) {
				Wrapped.HasDefault = hasDefault;
				Wrapped.HierarchyChanged -= HierarchyChanged;
			}
		}

		public override string ToString ()
		{
			if (Wrapped.Name != null)
				return "[" + Wrapped.GetType ().Name + " '" + Wrapped.Name + "' " + Wrapped.GetHashCode ().ToString () + "]";
			else
				return "[" + Wrapped.GetType ().Name + " " + Wrapped.GetHashCode ().ToString () + "]";
		}
	}
}

using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Widget : Object {

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

			ClassDescriptor klass = Registry.LookupClass (obj.GetType ());
			hexpandable = klass.HExpandable;
			vexpandable = klass.VExpandable;
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
				return Container.LookupParent (Wrapped);
			}
		}

		[GLib.ConnectBefore]
		void WidgetEvent (object obj, Gtk.WidgetEventArgs args)
		{
			if (args.Event.Type == Gdk.EventType.ButtonPress)
				args.RetVal = HandleClick ((Gdk.EventButton)args.Event);
		}

		internal bool HandleClick (Gdk.EventButton evb)
		{
			int x = (int)evb.X, y = (int)evb.Y;
			int erx, ery, wrx, wry;

			// Translate from event window to widget window coords
			evb.Window.GetOrigin (out erx, out ery);
			Wrapped.GdkWindow.GetOrigin (out wrx, out wry);
			x += erx - wrx;
			y += ery - wry;

			Widget wrapper = FindWrapper (Wrapped, x, y);
			if (wrapper == null)
				return false;

			if (wrapper.Wrapped != stetic.Selection) {
				wrapper.Select ();
				return true;
			} else if (evb.Button == 3) {
				stetic.PopupContextMenu (wrapper);
				return true;
			} else
				return false;
		}

		Widget FindWrapper (Gtk.Widget top, int x, int y)
		{
			Widget wrapper;

			Gtk.Container container = top as Gtk.Container;
			if (container != null) {
				foreach (Gtk.Widget child in container.AllChildren) {
					if (!child.IsDrawable)
						continue;

					Gdk.Rectangle alloc = child.Allocation;
					if (alloc.Contains (x, y)) {
						if (child.GdkWindow == top.GdkWindow)
							wrapper = FindWrapper (child, x, y);
						else
							wrapper = FindWrapper (child, x - alloc.X, y - alloc.Y);
						if (wrapper != null)
							return wrapper;
					}
				}
			}

			wrapper = Lookup (top);
			if (wrapper == null || wrapper.Unselectable)
				return null;
			return wrapper;
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

		public void Delete ()
		{
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

		bool hexpandable, vexpandable;
		public virtual bool HExpandable { get { return hexpandable; } }
		public virtual bool VExpandable { get { return vexpandable; } }

		public bool Unselectable;

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

		[GladeProperty]
		public bool Sensitive {
			get {
				return Wrapped.Sensitive;
			}
			set {
				if (Wrapped.Sensitive == value)
					return;

				Wrapped.Sensitive = value;
				if (Wrapped.Sensitive)
					InsensitiveManager.Remove (this);
				else
					InsensitiveManager.Add (this);
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

	internal static class InsensitiveManager {

		static Gtk.Invisible invis;
		static Hashtable map;

		static InsensitiveManager ()
		{
			map = new Hashtable ();
			invis = new Gtk.Invisible ();
			invis.ButtonPressEvent += ButtonPress;
		}

		static void ButtonPress (object obj, Gtk.ButtonPressEventArgs args)
		{
			Gtk.Widget widget = (Gtk.Widget)map[args.Event.Window];
			if (widget == null)
				return;

			Widget wrapper = Widget.Lookup (widget);
			args.RetVal = wrapper.HandleClick (args.Event);
		}

		public static void Add (Widget wrapper)
		{
			Gtk.Widget widget = wrapper.Wrapped;
			Gdk.WindowAttr attributes;

			attributes = new Gdk.WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.Wclass = Gdk.WindowClass.InputOnly;
			attributes.Mask = Gdk.EventMask.ButtonPressMask;

			Gdk.Window win = new Gdk.Window (widget.GdkWindow, attributes, 0);
			win.UserData = invis.Handle;
			win.MoveResize (widget.Allocation);
			win.Show ();

			map[widget] = win;
			map[win] = widget;
			widget.SizeAllocated += Insensitive_SizeAllocate;
			widget.Mapped += Insensitive_Mapped;
			widget.Unmapped += Insensitive_Unmapped;
		}

		public static void Remove (Widget wrapper)
		{
			Gtk.Widget widget = wrapper.Wrapped;
			Gdk.Window win = (Gdk.Window)map[widget];
			if (win == null)
				return;

			map.Remove (widget);
			map.Remove (win);
			win.Destroy ();
			widget.SizeAllocated -= Insensitive_SizeAllocate;
			widget.Mapped -= Insensitive_Mapped;
			widget.Unmapped -= Insensitive_Unmapped;
		}

		static void Insensitive_SizeAllocate (object obj, Gtk.SizeAllocatedArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.MoveResize (args.Allocation);
		}

		static void Insensitive_Mapped (object obj, EventArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.Show ();
		}

		static void Insensitive_Unmapped (object obj, EventArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.Hide ();
		}
	}
}

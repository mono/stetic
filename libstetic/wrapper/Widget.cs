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

			Wrapped.Events |= Gdk.EventMask.ButtonPressMask;
			Wrapped.WidgetEvent += WidgetEvent;
			Wrapped.PopupMenu += PopupMenu;
		}

		public new Gtk.Widget Wrapped {
			get {
				return (Gtk.Widget)base.Wrapped;
			}
		}

		public Stetic.Wrapper.Container ParentWrapper {
			get {
				return Stetic.Wrapper.Container.Lookup (Wrapped.Parent);
			}
		}

		[GLib.ConnectBefore]
		void WidgetEvent (object obj, Gtk.WidgetEventArgs args)
		{
			if (args.Event.Type != Gdk.EventType.ButtonPress)
				return;

			Gdk.EventButton evb = new Gdk.EventButton (args.Event.Handle);
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

			foreach (Gtk.Widget child in container.Children) {
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

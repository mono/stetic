using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public abstract class WidgetBox : Gtk.Bin {

		public WidgetBox ()
		{
			WidgetFlags &= ~WidgetFlags.NoWindow;
		}

		public abstract bool HExpandable { get; }
		public abstract bool VExpandable { get; }

		bool isInternal;
		public virtual bool Internal {
			get { return isInternal; }
			set {
				if (value == isInternal)
					return;
				isInternal = value;
			}
		}

		public event EventHandler PopupContextMenu;

		public void EmitPopupContextMenu ()
		{
			if (PopupContextMenu != null)
				PopupContextMenu (this, EventArgs.Empty);
		}

		protected override bool OnPopupMenu ()
		{
			EmitPopupContextMenu ();
			return true;
		}

		protected override void OnSizeRequested (ref Requisition req)
		{
			if (Child == null)
				req.Width = req.Height = 10;
			else
				req = Child.SizeRequest ();
		}

		protected override void OnSizeAllocated (Rectangle allocation)
		{
			Allocation = allocation;

			if (GdkWindow != null && GdkWindow != ParentWindow)
				GdkWindow.MoveResize (allocation);

			if (Child != null) {
				allocation.X = allocation.Y = 0;
				Child.SizeAllocate (allocation);
			}
		}

		protected Gdk.Window NewWindow (Gdk.Window parent, Gdk.WindowClass wclass)
		{
			WindowAttr attributes;
			WindowAttributesType attributesMask;
			Gdk.Window win;

			attributes = new WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.X = Allocation.X;
			attributes.Y = Allocation.Y;
			attributes.Width = Allocation.Width;
			attributes.Height = Allocation.Height;
			attributes.Wclass = wclass ;
			attributes.visual = Visual;
			attributes.colormap = Colormap;
			attributes.EventMask = (int)(Events |
						     EventMask.ButtonPressMask |
						     EventMask.ButtonMotionMask |
						     EventMask.ButtonReleaseMask |
						     EventMask.ExposureMask |
						     EventMask.EnterNotifyMask |
						     EventMask.LeaveNotifyMask);

			attributesMask =
				Gdk.WindowAttributesType.X |
				Gdk.WindowAttributesType.Y |
				Gdk.WindowAttributesType.Visual |
				Gdk.WindowAttributesType.Colormap;

			win = new Gdk.Window (parent, attributes, attributesMask);
			win.UserData = Handle;

			if (wclass == Gdk.WindowClass.InputOutput)
				Style.Attach (win);

			return win;
		}

		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;

			GdkWindow = NewWindow (ParentWindow, Gdk.WindowClass.InputOutput);
			Style.SetBackground (GdkWindow, StateType.Normal);
		}

		protected override void OnSetScrollAdjustments (Gtk.Adjustment hadj, Gtk.Adjustment vadj)
		{
			if (Child != null)
				Child.SetScrollAdjustments (hadj, vadj);
		}
	}
}

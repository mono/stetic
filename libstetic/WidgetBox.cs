using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public abstract class WidgetBox : Gtk.Bin {

		static Color black, white;

		static WidgetBox ()
		{
			black = new Color (0, 0, 0);
			black.Pixel = 1;
			white = new Color (255, 255, 255);
			white.Pixel = 0;
		}

		public WidgetBox ()
		{
			WidgetFlags |= WidgetFlags.CanFocus;
			WidgetFlags &= ~WidgetFlags.NoWindow;
		}

		public abstract bool HExpandable { get; }
		public abstract bool VExpandable { get; }

		public Gdk.Window HandleWindow;
		bool showHandles;
		protected bool ShowHandles {
			get { return showHandles; }
			set {
				if (value == showHandles)
					return;
				showHandles = value;

				if (showHandles) {
					if (IsRealized) {
						HandleWindow = NewWindow (Toplevel.GdkWindow, Gdk.WindowClass.InputOutput);
						HandleWindow.Background = black;
						ShapeHandles ();
					}
					if (IsMapped)
						HandleWindow.Show ();
				} else {
					if (HandleWindow != null) {
						HandleWindow.Hide ();
						HandleWindow.Destroy ();
						HandleWindow = null;
					}
				}
			}
		}

		bool isInternal;
		public virtual bool Internal {
			get { return isInternal; }
			set {
				if (value == isInternal)
					return;
				isInternal = value;

				if (ShowHandles) {
					ShowHandles = false;
					ShowHandles = true;
				}
			}
		}

		protected override void OnAdded (Widget child)
		{
			child.ButtonPressEvent += InterceptButtonPress;
			base.OnAdded (child);
		}

		protected override void OnRemoved (Widget child)
		{
			child.ButtonPressEvent -= InterceptButtonPress;
			ShowHandles = false;
			base.OnRemoved (child);
		}

		public event EventHandler PopupContextMenu;

		protected void EmitPopupContextMenu ()
		{
			if (PopupContextMenu != null)
				PopupContextMenu (this, EventArgs.Empty);
		}

		[ConnectBefore]
		void InterceptButtonPress (object obj, ButtonPressEventArgs args)
		{
			if (args.Event.Type != EventType.ButtonPress)
				return;

			if (args.Event.Button == 1 && !ShowHandles) {
				GrabFocus ();
				args.RetVal = true;
			} else if (args.Event.Button == 3) {
				args.RetVal = true;
				EmitPopupContextMenu ();
			}
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{

			if (evt.Button == 3 && evt.Type == EventType.ButtonPress)
				EmitPopupContextMenu ();
			else
				GrabFocus ();
			return true;
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
			if (HandleWindow != null)
				ShapeHandles ();

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

			if (ShowHandles) {
				HandleWindow = NewWindow (Toplevel.GdkWindow, Gdk.WindowClass.InputOutput);
				HandleWindow.Background = black;
				ShapeHandles ();
			}
		}

		private const int handleSize = 6;
		void ShapeHandles ()
		{
			Gdk.GC gc;
			Gdk.Pixmap pixmap;
			Rectangle handleAllocation;
			int tlx, tly;

			TranslateCoordinates (Toplevel, 0, 0, out tlx, out tly);
			handleAllocation.X = tlx - handleSize / 2;
			handleAllocation.Y = tly - handleSize / 2;
			handleAllocation.Width = Allocation.Width + handleSize;
			handleAllocation.Height = Allocation.Height + handleSize;

			int width = handleAllocation.Width, height = handleAllocation.Height;

			pixmap = new Pixmap (HandleWindow, width, height, 1);
			gc = new Gdk.GC (pixmap);
			gc.Background = white;
			gc.Foreground = white;
			pixmap.DrawRectangle (gc, true, 0, 0, width, height);

			gc.Foreground = black;

			// Draw border
			pixmap.DrawRectangle (gc, false, handleSize / 2, handleSize / 2,
					      width - handleSize, height - handleSize);

			if (!Internal) {
				// Draw corner handles
				pixmap.DrawRectangle (gc, true, 0, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, 0, height - handleSize, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, height - handleSize, handleSize, handleSize);
			}

			HandleWindow.MoveResize (handleAllocation);
			HandleWindow.ShapeCombineMask (pixmap, 0, 0);
		}

		protected override void OnMapped ()
		{
			base.OnMapped ();
			if (HandleWindow != null)
				HandleWindow.Show ();
		}

		protected override void OnUnmapped ()
		{
			if (HandleWindow != null)
				HandleWindow.Hide ();
			base.OnUnmapped ();
		}

		protected override void OnUnrealized ()
		{
			if (HandleWindow != null) {
				HandleWindow.Destroy ();
				HandleWindow = null;
			}
			base.OnUnrealized ();
		}

		protected override void OnSetScrollAdjustments (Gtk.Adjustment hadj, Gtk.Adjustment vadj)
		{
			if (Child != null)
				Child.SetScrollAdjustments (hadj, vadj);
		}

		public event EventHandler Selected;

		public void Focus ()
		{
			ShowHandles = true;
			if (Selected != null)
				Selected (this, EventArgs.Empty);
		}

		public void UnFocus ()
		{
			ShowHandles = false;
		}
	}
}

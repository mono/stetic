//
// PreviewBox.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using Gtk;
using Gdk;

namespace Stetic
{	
	public class PreviewBox: ScrolledWindow
	{
		Gtk.Widget preview;
		Metacity.Preview metacityPreview;
		
		internal Metacity.Theme Theme {
			set { 
				if (metacityPreview != null)
					metacityPreview.Theme = value;
			}
		}
		
		internal string Title {
			set { 
				if (metacityPreview != null)
					metacityPreview.Title = value;
			}
		}
		
		internal PreviewBox (Gtk.Container container, int designWidth, int designHeight)
		{
			ShadowType = ShadowType.None;
			HscrollbarPolicy = PolicyType.Automatic;
			VscrollbarPolicy = PolicyType.Automatic;
			
			ResizableFixed fixd = new ResizableFixed ();
			
			Gtk.Window window = container as Gtk.Window;
			
			if (window != null) {
				try {
					metacityPreview = CreateMetacityPreview (window);
					preview = metacityPreview;
				} catch {
					// If metacity is not available, use a regular box.
					EventBox eventBox = new EventBox ();
					eventBox.Add (container);
					preview = eventBox;
				}

			} else {
				EventBox eventBox = new EventBox ();
				eventBox.Add (container);
				preview = eventBox;
			}
			
			fixd.Put (preview, container);

			preview.WidthRequest = designWidth;
			preview.HeightRequest = designHeight;
			preview.SizeAllocated += new Gtk.SizeAllocatedHandler (OnResized);
				
			AddWithViewport (fixd);
		}
		
		Metacity.Preview CreateMetacityPreview (Gtk.Window window)
		{
			Metacity.Preview metacityPreview = new Metacity.Preview ();
			
			switch (window.TypeHint) {
			case Gdk.WindowTypeHint.Normal:
				metacityPreview.FrameType = Metacity.FrameType.Normal;
				break;
			case Gdk.WindowTypeHint.Dialog:
				metacityPreview.FrameType = window.Modal ? Metacity.FrameType.ModalDialog : Metacity.FrameType.Dialog;	
				break;
			case Gdk.WindowTypeHint.Menu:
				metacityPreview.FrameType = Metacity.FrameType.Menu;
				break;
			case Gdk.WindowTypeHint.Splashscreen:
				metacityPreview.FrameType = Metacity.FrameType.Border;
				break;
			case Gdk.WindowTypeHint.Utility:
				metacityPreview.FrameType = Metacity.FrameType.Utility;
				break;
			default:
				metacityPreview.FrameType = Metacity.FrameType.Normal;
				break;
			}

			Metacity.FrameFlags flags =
				Metacity.FrameFlags.AllowsDelete |
				Metacity.FrameFlags.AllowsVerticalResize |
				Metacity.FrameFlags.AllowsHorizontalResize |
				Metacity.FrameFlags.AllowsMove |
				Metacity.FrameFlags.AllowsShade |
				Metacity.FrameFlags.HasFocus;
				
			if (window.Resizable)
				flags = flags | Metacity.FrameFlags.AllowsMaximize;
				
			metacityPreview.FrameFlags = flags;
			metacityPreview.ShowAll ();
			metacityPreview.Add (window);
			return metacityPreview;
		}
		
		protected override void OnParentSet (Gtk.Widget previousParent)
		{
			base.OnParentSet (previousParent);
			
			if (previousParent != null)
				previousParent.Realized -= OnParentRealized;
			
			if (Parent != null)
				Parent.Realized += OnParentRealized;
		}
		
		void OnParentRealized (object s, EventArgs args)
		{
			if (Parent != null) {
				Parent.Realized -= OnParentRealized;
				ShowAll ();
				
				// Make sure everything is in place before continuing
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
			}
		}
		
		void OnResized (object s, Gtk.SizeAllocatedArgs a)
		{
			if (DesignSizeChanged != null)
				DesignSizeChanged (this, a);
		}
		
		public int DesignWidth {
			get { return preview.WidthRequest; }
		}
		
		public int DesignHeight {
			get { return preview.HeightRequest; }
		}
		
		public event EventHandler DesignSizeChanged;
	}
	
	class ResizableFixed: EventBox
	{
		Gtk.Widget child;
		int difX, difY;
		bool resizingX;
		bool resizingY;
		Fixed fixd;
		Gtk.Container container;
		
		Cursor cursorX = new Cursor (CursorType.RightSide);
		Cursor cursorY = new Cursor (CursorType.BottomSide);
		Cursor cursorXY = new Cursor (CursorType.BottomRightCorner);
		
		const int padding = 6;
		const int selectionBorder = 6;
		
		Requisition currentSizeRequest;
		
		public ResizableFixed ()
		{
			fixd = new Fixed ();
			Add (fixd);
			this.Events = EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.PointerMotionMask;
//			fixd.ModifyBg (Gtk.StateType.Normal, this.Style.Mid (Gtk.StateType.Normal));
//			VisibleWindow = false;
		}
		
		public void Put (Gtk.Widget child, Gtk.Container container)
		{
			this.child = child;
			this.container = container;
			fixd.Put (child, selectionBorder + padding, selectionBorder + padding);
			child.SizeRequested += new SizeRequestedHandler (OnSizeReq);
		}
		
		void OnSizeReq (object o, SizeRequestedArgs a)
		{
			currentSizeRequest = a.Requisition;
			
			Rectangle alloc = child.Allocation;
			int nw = alloc.Width;
			int nh = alloc.Height;
			
			if (a.Requisition.Width > nw) nw = a.Requisition.Width;
			if (a.Requisition.Height > nh) nh = a.Requisition.Height;
			
			if (nw != alloc.Width || nh != alloc.Height) {
				int ow = child.WidthRequest;
				int oh = child.HeightRequest;
				child.SetSizeRequest (nw, nh);
				if (ow > nw)
					child.WidthRequest = ow;
				if (oh > nh)
					child.HeightRequest = oh;
				QueueDraw ();
			}
		}
		
		protected override void OnSizeRequested (ref Requisition req)
		{
			req = child.SizeRequest ();
			// Make some room for the border
			req.Width += padding * 2 + selectionBorder * 2;
			req.Height += padding * 2 + selectionBorder * 2;
		}
		
		protected override bool OnMotionNotifyEvent (Gdk.EventMotion ev)
		{
			if (resizingX || resizingY) {
				if (resizingX) {
					int nw = (int)(ev.X - difX - padding - selectionBorder);
					if (nw < currentSizeRequest.Width) nw = currentSizeRequest.Width;
					child.WidthRequest = nw;
				}
				
				if (resizingY) {
					int nh = (int)(ev.Y - difY - padding - selectionBorder);
					if (nh < currentSizeRequest.Height) nh = currentSizeRequest.Height;
					child.HeightRequest = nh;
				}
				QueueDraw ();
			} else {
				if (GetAreaResizeXY ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorXY;
				else if (GetAreaResizeX ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorX;
				else if (GetAreaResizeY ().Contains ((int) ev.X, (int) ev.Y))
					GdkWindow.Cursor = cursorY;
				else
					GdkWindow.Cursor = null;
			}
			
			return base.OnMotionNotifyEvent (ev);
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			Gdk.Rectangle rectArea = child.Allocation;
			rectArea.Inflate (selectionBorder, selectionBorder);
			
			if (rectArea.Contains ((int) ev.X, (int) ev.Y)) {
				Stetic.Wrapper.Widget gw = Stetic.Wrapper.Widget.Lookup (container);
				gw.Select ();
				
				Rectangle rect = GetAreaResizeXY ();
				if (rect.Contains ((int) ev.X, (int) ev.Y)) {
					resizingX = resizingY = true;
					difX = (int) (ev.X - rect.X);
					difY = (int) (ev.Y - rect.Y);
					GdkWindow.Cursor = cursorXY;
				}
				
				rect = GetAreaResizeY ();
				if (rect.Contains ((int) ev.X, (int) ev.Y)) {
					resizingY = true;
					difY = (int) (ev.Y - rect.Y);
					GdkWindow.Cursor = cursorY;
				}
				
				rect = GetAreaResizeX ();
				if (rect.Contains ((int) ev.X, (int) ev.Y)) {
					resizingX = true;
					difX = (int) (ev.X - rect.X);
					GdkWindow.Cursor = cursorX;
				}
			} else {
				Stetic.Wrapper.Widget gw = Stetic.Wrapper.Widget.Lookup (container);
				gw.Project.Selection = null;
			}
			
			return base.OnButtonPressEvent (ev);
		}
		
		Rectangle GetAreaResizeY ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X - selectionBorder, rect.Y + rect.Height, rect.Width + selectionBorder, selectionBorder);
		}
		
		Rectangle GetAreaResizeX ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width, rect.Y - selectionBorder, selectionBorder, rect.Height + selectionBorder);
		}
		
		Rectangle GetAreaResizeXY ()
		{
			Gdk.Rectangle rect = child.Allocation;
			return new Gdk.Rectangle (rect.X + rect.Width, rect.Y + rect.Height, selectionBorder, selectionBorder);
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton ev)
		{
			resizingX = resizingY = false;
			GdkWindow.Cursor = null;
			return base.OnButtonReleaseEvent (ev);
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			bool r = base.OnExposeEvent (ev);
			
			//this.Style.DarkGC (Gtk.StateType.Normal)
//			GdkWindow.DrawRectangle (Style.MidGC (Gtk.StateType.Normal), true, 0, 0, 1000, 1000);
			
			Gdk.Rectangle rect = child.Allocation;
/*			rect.Inflate (selectionBorder, selectionBorder);
			GdkWindow.DrawRectangle (Style.BlackGC, false, rect.X, rect.Y, rect.Width, rect.Height);
*/			
			Pixbuf sh = Shadow.AddShadow (rect.Width, rect.Height);
			GdkWindow.DrawPixbuf (this.Style.BackgroundGC (StateType.Normal), sh, 0, 0, rect.X - 5, rect.Y - 5, sh.Width, sh.Height, RgbDither.None, 0, 0); 
			return r;
		}
	}
}

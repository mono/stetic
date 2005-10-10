using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public static class EmbedWindow {

		static Hashtable wrappers = new Hashtable ();

		public static Widget Wrap (Window window)
		{
			if (wrappers[window] != null)
				return (Gtk.Widget)wrappers[window];

			if (window.IsRealized)
				throw new ApplicationException ("Cannot make a realized window embeddable");

			window.WidgetFlags &= ~(WidgetFlags.Toplevel);
			window.SizeAllocated += new SizeAllocatedHandler (OnSizeAllocated);
			window.Realized += new EventHandler (OnRealized);

			Metacity.Preview prev = new Metacity.Preview ();
			prev.Title = window.Title;
			prev.Theme = Theme;

			switch (window.TypeHint) {
			case Gdk.WindowTypeHint.Normal:
				prev.FrameType = Metacity.FrameType.Normal;
				break;
			case Gdk.WindowTypeHint.Dialog:
				prev.FrameType = window.Modal ? Metacity.FrameType.ModalDialog : Metacity.FrameType.Dialog;	
				break;
			case Gdk.WindowTypeHint.Menu:
				prev.FrameType = Metacity.FrameType.Menu;
				break;
			case Gdk.WindowTypeHint.Splashscreen:
				prev.FrameType = Metacity.FrameType.Border;
				break;
			case Gdk.WindowTypeHint.Utility:
				prev.FrameType = Metacity.FrameType.Utility;
				break;
			default:
				prev.FrameType = Metacity.FrameType.Normal;
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
			prev.FrameFlags = flags;

			prev.Add (window);

			wrappers[window] = prev;
			return prev;
		}

		static private void OnSizeAllocated (object obj, SizeAllocatedArgs args)
		{
			Window window = obj as Window;
			Gdk.Rectangle allocation = args.Allocation;

			if (window.IsRealized) {
				window.GdkWindow.MoveResize (allocation.X,
							     allocation.Y,
							     allocation.Width,
							     allocation.Height);
			}
		}

		static private void OnRealized (object obj, EventArgs args)
		{
			Window window = obj as Window;

			window.WidgetFlags |= WidgetFlags.Realized;

			Gdk.WindowAttr attrs = new Gdk.WindowAttr ();
			attrs.Mask = window.Events |
				(Gdk.EventMask.ExposureMask |
				 Gdk.EventMask.KeyPressMask |
				 Gdk.EventMask.KeyReleaseMask |
				 Gdk.EventMask.EnterNotifyMask |
				 Gdk.EventMask.LeaveNotifyMask |
				 Gdk.EventMask.StructureMask);
			attrs.X = window.Allocation.X;
			attrs.Y = window.Allocation.Y;
			attrs.Width = window.Allocation.Width;
			attrs.Height = window.Allocation.Height;
			attrs.Wclass = Gdk.WindowClass.InputOutput;
			attrs.Visual = window.Visual;
			attrs.Colormap = window.Colormap;
			attrs.WindowType = Gdk.WindowType.Child;

			Gdk.WindowAttributesType mask =
				Gdk.WindowAttributesType.X |
				Gdk.WindowAttributesType.Y |
				Gdk.WindowAttributesType.Colormap |
				Gdk.WindowAttributesType.Visual;

			window.GdkWindow = new Gdk.Window (window.ParentWindow, attrs, mask);
			window.GdkWindow.UserData = window.Handle;

			window.Style = window.Style.Attach (window.GdkWindow);
			window.Style.SetBackground (window.GdkWindow, StateType.Normal);

			// FIXME: gtk-sharp 2.6
			// window.GdkWindow.EnableSynchronizedConfigure ();
		}

		static Metacity.Theme theme;
		static Metacity.Theme Theme {
			get {
				if (theme == null) {
					GConf.Client client = new GConf.Client ();
					client.AddNotify ("/apps/metacity/general", GConfNotify);
					string themeName = (string)client.Get ("/apps/metacity/general/theme");
					theme = Metacity.Theme.Load (themeName);
				}
				return theme;
			}
		}

		static void GConfNotify (object obj, GConf.NotifyEventArgs args)
		{
			if (args.Key == "/apps/metacity/general/theme") {
				theme = Metacity.Theme.Load ((string)args.Value);
				foreach (Metacity.Preview prev in wrappers.Values)
					prev.Theme = Theme;
			}
		}
	}
}

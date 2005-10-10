using System;
using Gtk;
using Metacity;

class TestPreview {

	public static void Main ()
	{
		Application.Init ();

		Gtk.Window top = new Gtk.Window ("Preview Test");
		top.DeleteEvent += WindowDeleted;
		top.BorderWidth = 6;
		top.Show ();

		Fixed layout = new Fixed ();
		top.Add (layout);

		EventBox ebox = new EventBox ();
		layout.Put (ebox, 10, 10);
		ebox.ButtonPressEvent += PreviewButtonPress;
		ebox.MotionNotifyEvent += PreviewDrag;
		ebox.ButtonReleaseEvent += PreviewButtonRelease;

		/*MessageDialog dlg = new MessageDialog
			(null,
			 DialogFlags.Modal | DialogFlags.DestroyWithParent,
			 MessageType.Info,
			 ButtonsType.Ok,
			 "{0} ({1})",
			 "<b>Hello</b>",
			 "<i>World!</i>");
		dlg.Title = "MessageDialog";*/
		
		FileChooserDialog dlg = new FileChooserDialog ("Open File",
							       null,
							       FileChooserAction.Open,
							       new object[] {});
		dlg.AddButton (Stock.Cancel, ResponseType.Cancel);
		dlg.AddButton (Stock.Open, ResponseType.Ok);
		dlg.DefaultResponse = ResponseType.Ok;
		
		MakeEmbeddable (dlg);

		Preview prev = CreatePreview (dlg);
		ebox.Add (prev);
		prev.Add (dlg);

		top.ShowAll ();
		Application.Run ();
	}
	
	static Preview CreatePreview (Window window)
	{
		Preview prev = new Preview ();
		prev.Title = window.Title;
		prev.Theme = Theme.Load ("Office");
		
		switch (window.TypeHint) {
			case Gdk.WindowTypeHint.Normal:
				prev.FrameType = FrameType.Normal;
				break;
			case Gdk.WindowTypeHint.Dialog:
				prev.FrameType = window.Modal ? FrameType.ModalDialog : FrameType.Dialog;	
				break;
			case Gdk.WindowTypeHint.Menu:
				prev.FrameType = FrameType.Menu;
				break;
			case Gdk.WindowTypeHint.Splashscreen:
				prev.FrameType = FrameType.Border;
				break;
			case Gdk.WindowTypeHint.Utility:
				prev.FrameType = FrameType.Utility;
				break;
			default:
				prev.FrameType = FrameType.Normal;
				break;
		}
		
		FrameFlags flags =
			FrameFlags.AllowsDelete |
			FrameFlags.AllowsVerticalResize |
			FrameFlags.AllowsHorizontalResize |
			FrameFlags.AllowsMove |
			FrameFlags.AllowsShade |
			FrameFlags.HasFocus;
			
		if (window.Resizable)
			flags = flags | FrameFlags.AllowsMaximize;
		
		prev.FrameFlags = flags;
		
		return prev;
	}
	
	static void MakeEmbeddable (Window window)
	{
		if (window.IsRealized)
			throw new ApplicationException ("Cannot make a realized window embeddable");
		
		window.WidgetFlags &= ~(WidgetFlags.Toplevel);
		window.SizeAllocated += new SizeAllocatedHandler (OnSizeAllocated);
		window.Realized += new EventHandler (OnRealized);
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
		
//		FIXME: gtk-sharp 2.6
//		window.GdkWindow.EnableSynchronizedConfigure ();
	}

	static void WindowDeleted (object obj, DeleteEventArgs args)
	{
		Application.Quit ();
	}

	static void PreviewButtonPress (object obj, ButtonPressEventArgs args)
	{
		Console.WriteLine ("PreviewButtonPress");
	}

	static void PreviewButtonRelease (object obj, ButtonReleaseEventArgs args)
	{
		Console.WriteLine ("PreviewButtonRelease");
	}

	static void PreviewDrag (object obj, MotionNotifyEventArgs args)
	{
		Console.WriteLine ("PreviewMotion");
	}
}

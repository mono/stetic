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

		Preview prev = new Preview ();
		ebox.Add (prev);
		prev.Title = "Sample window";
		prev.Theme = Theme.Load ("Office");
		prev.FrameType = FrameType.Normal;
		prev.FrameFlags =
			FrameFlags.AllowsDelete |
			FrameFlags.AllowsVerticalResize |
			FrameFlags.AllowsHorizontalResize |
			FrameFlags.AllowsMove |
			FrameFlags.AllowsShade;

		MessageDialog md = new MessageDialog
			(null,
			 DialogFlags.Modal | DialogFlags.DestroyWithParent,
			 MessageType.Info,
			 ButtonsType.Ok,
			 "{0} ({1})",
			 "<b>Hello</b>",
			 "<i>World!</i>");
		md.Title = "MessageDialog";
		MakeEmbeddable (md);

		prev.Add (md);

		top.ShowAll ();
		Application.Run ();
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
		
		window.SizeAllocate (allocation);
		
		if (window.IsRealized) {
			window.GdkWindow.MoveResize (allocation.X,
						     allocation.Y,
						     allocation.Width,
						     allocation.Height);
		}
		
		if (window.Child != null && window.Child.Visible) {
			Gdk.Rectangle childAllocation;
			childAllocation.X = (int) window.BorderWidth;
			childAllocation.Y = (int) window.BorderWidth;
			childAllocation.Width = Math.Max (1, allocation.Width - ((int) window.BorderWidth * 2));
			childAllocation.Height = Math.Max (1, allocation.Height - ((int) window.BorderWidth * 2));
			window.Child.SizeAllocate (childAllocation);
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
		
		window.GdkWindow.EnableSynchronizedConfigure ();
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

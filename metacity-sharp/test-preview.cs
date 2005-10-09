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
		prev.Theme = Theme.Load ("Clearlooks");
		prev.FrameType = FrameType.Normal;
		prev.FrameFlags =
			FrameFlags.AllowsDelete |
			FrameFlags.AllowsVerticalResize |
			FrameFlags.AllowsHorizontalResize |
			FrameFlags.AllowsMove |
			FrameFlags.AllowsShade;

		Button ok = new Gtk.Button (Gtk.Stock.Ok);
		prev.Add (ok);

		top.ShowAll ();
		Application.Run ();
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

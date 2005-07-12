using System;

namespace Stetic {
	public class Custom : Gtk.DrawingArea {
		public Custom () {}

		public Custom (IntPtr raw) : base (raw) {}

		// from glade
		static private string[] custom_bg_xpm = {
			"8 8 4 1",
			" 	c None",
			".	c #BBBBBB",
			"+	c #D6D6D6",
			"@	c #6B5EFF",
			".+..+...",
			"+..@@@..",
			"..@...++",
			"..@...++",
			"+.@..+..",
			".++@@@..",
			"..++....",
			"..++...."
		};

		protected override void OnRealized ()
		{
			base.OnRealized ();

			Gdk.Pixmap pixmap, mask;
			pixmap = Gdk.Pixmap.CreateFromXpmD (GdkWindow, out mask, new Gdk.Color (99, 99, 99), custom_bg_xpm);
			GdkWindow.SetBackPixmap (pixmap, false);
		}

		string creationFunction, string1, string2;
		int int1, int2;

		public string CreationFunction {
			get {
				return creationFunction;
			}
			set {
				creationFunction = value;
			}
		}

		public string LastModificationTime {
			get {
				return null;
			}
			set {
				;
			}
		}

		public string String1 {
			get {
				return string1;
			}
			set {
				string1 = value;
			}
		}

		public string String2 {
			get {
				return string2;
			}
			set {
				string2 = value;
			}
		}

		public int Int1 {
			get {
				return int1;
			}
			set {
				int1 = value;
			}
		}

		public int Int2 {
			get {
				return int2;
			}
			set {
				int2 = value;
			}
		}
	}
}

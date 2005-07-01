using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {
	public class Custom : Gtk.DrawingArea {
		static Custom ()
		{
			GLib.GType.Register (GType, typeof (Stetic.Custom));
		}

		public Custom (IntPtr raw) : base(raw) {}

		[DllImport("libsteticglue")]
		static extern IntPtr custom_new();

		public Custom () : base (IntPtr.Zero)
		{
			Raw = custom_new ();
		}

		~Custom () { Dispose (); }

		[DllImport("libsteticglue")]
		static extern IntPtr custom_get_type();

		public static new GLib.GType GType { 
			get {
				return new GLib.GType (custom_get_type ());
			}
		}
	}
}

namespace Stetic.Wrapper {

	public class Custom : Widget {

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

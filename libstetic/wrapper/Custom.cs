using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {
	public class Custom : Gtk.DrawingArea {
		static Custom ()
		{
			GLib.ObjectManager.RegisterType (GType, typeof (Stetic.Custom));
		}

		public Custom (IntPtr raw) : base(raw) {}
		public Custom () : base (IntPtr.Zero) {}

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

	[ObjectWrapper ("Custom Widget", "custom.png", ObjectWrapperType.Widget)]
	public class Custom : DrawingArea {

		public static new Type WrappedType = typeof (Stetic.Custom);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Custom Widget Properties",
				      "CreationFunction",
				      "String1",
				      "String2",
				      "Int1",
				      "Int2");
		}

		string creationFunction, string1, string2, int1 = "0", int2 = "0";

		[GladeProperty (Name = "creation_function")]
		public string CreationFunction {
			get {
				return creationFunction;
			}
			set {
				creationFunction = value;
			}
		}

		[GladeProperty (Name = "string1")]
		public string String1 {
			get {
				return string1;
			}
			set {
				string1 = value;
			}
		}

		[GladeProperty (Name = "string2")]
		public string String2 {
			get {
				return string2;
			}
			set {
				string2 = value;
			}
		}

		// This is sort of lame. But the properties were actually int-valued,
		// they would need GladeProxies. We should move all of these into
		// custom.c anyway.

		[GladeProperty (Name = "int1")]
		public string Int1 {
			get {
				return int1;
			}
			set {
				try {
					int1 = Int32.Parse (value).ToString ();
				} catch {
					int1 = "0";
				}
			}
		}

		[GladeProperty (Name = "int2")]
		public string Int2 {
			get {
				return int2;
			}
			set {
				try {
					int2 = Int32.Parse (value).ToString ();
				} catch {
					int2 = "0";
				}
			}
		}
	}
}

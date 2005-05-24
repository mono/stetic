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

	[ObjectWrapper ("Custom Widget", "custom.png", ObjectWrapperType.Widget)]
	public class Custom : DrawingArea {

		public static new Type WrappedType = typeof (Stetic.Custom);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Custom Widget Properties",
				      "CreationFunction",
				      "String1",
				      "String2",
				      "Int1",
				      "Int2");
		}

		string creationFunction, string1, string2;
		int int1, int2;

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

		[GladeProperty (Name = "int1")]
		public int Int1 {
			get {
				return int1;
			}
			set {
				int1 = value;
			}
		}

		[GladeProperty (Name = "int2")]
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

using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Entry", "entry.png", ObjectWrapperType.Widget)]
	public class Entry : Widget {

		public static new Type WrappedType = typeof (Gtk.Entry);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Entry Properties",
				      "Text",
				      "Editable",
				      "WidthChars",
				      "MaxLength",
				      "HasFrame",
				      "ActivatesDefault",
				      "Visibility");
			AddItemGroup (type, "Extra Entry Properties",
				      "InvisibleChar",
				      "Xalign");
		}

		[GladeProperty (Proxy = "GladeInvisibleChar")]
		public char InvisibleChar {
			get {
				return ((Gtk.Entry)Wrapped).InvisibleChar;
			}
			set {
				((Gtk.Entry)Wrapped).InvisibleChar = value;
			}
		}

		private string GladeInvisibleChar {
			get {
				return InvisibleChar.ToString ();
			}
			set {
				InvisibleChar = Char.Parse (value);
			}
		}
	}
}

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

		protected override void GladeImport (string className, string id, ArrayList propNames, ArrayList propVals)
		{
			int index = propNames.IndexOf ("invisible_char");
			if (index != -1) {
				// invisible_char is a guint, but glade
				// serializes it as a string, so we have
				// to translate it
				string val = propVals[index] as string;
				propVals[index] = ((int)val[0]).ToString ();
			}

			base.GladeImport (className, id, propNames, propVals);
		}
	}
}

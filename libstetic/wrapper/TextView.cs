using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Text View", "textview.png", ObjectWrapperType.Widget)]
	public class TextView : Container {

		public static new Type WrappedType = typeof (Gtk.TextView);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Text View Properties",
				      "Editable",
				      "CursorVisible",
				      "Overwrite",
				      "AcceptsTab",
				      "Tabs",
				      "Text",
				      "Justification",
				      "WrapMode",
				      "PixelsAboveLines",
				      "PixelsBelowLines",
				      "PixelsInsideWrap",
				      "RightMargin",
				      "LeftMargin",
				      "Indent");
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.TextView)Wrapped).Buffer.Changed += Buffer_Changed;
		}

		protected override void GladeImport (string className, string id, ArrayList propNames, ArrayList propVals)
		{
			string text = GladeUtils.ExtractProperty ("text", propNames, propVals);
			base.GladeImport (className, id, propNames, propVals);
			Text = text;
		}

		[Editor (typeof (Stetic.Editor.Text))]
		[Description ("Text", "The initial text to display in the Text View")]
		public string Text {
			get {
				return ((Gtk.TextView)Wrapped).Buffer.Text;
			}
			set {
				((Gtk.TextView)Wrapped).Buffer.Text = value;
			}
		}

		public void Buffer_Changed (object obj, EventArgs args)
		{
			EmitNotify ("Text");
		}
	}
}

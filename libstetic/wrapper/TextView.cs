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

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.TextView)Wrapped).Buffer.Changed += Buffer_Changed;
		}

		[Editor (typeof (Stetic.Editor.Text))]
		[Description ("Text", "The initial text to display in the Text View")]
		[GladeProperty (Name = "text")]
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

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }
	}
}

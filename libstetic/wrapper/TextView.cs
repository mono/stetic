using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Text View", "textview.png", ObjectWrapperType.Widget)]
	public class TextView : Stetic.Wrapper.Widget {

		public static ItemGroup TextViewProperties;
		public static ItemGroup TextViewExtraProperties;

		static TextView () {
			TextViewProperties = new ItemGroup ("Text View Properties",
							    typeof (Stetic.Wrapper.TextView),
							    typeof (Gtk.TextView),
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
			RegisterWrapper (typeof (Stetic.Wrapper.TextView),
					 TextViewProperties,
					 Widget.CommonWidgetProperties);
		}

		public TextView (IStetic stetic) : this (stetic, new Gtk.TextView ()) {}

		public TextView (IStetic stetic, Gtk.TextView textview) : base (stetic, textview)
		{
			textview.Buffer.Changed += Buffer_Changed;
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

using System;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class Text : Translatable {

		Stetic.TextBox textbox;

		public Text (PropertyDescriptor prop, object obj) : base (prop, obj)
		{
			textbox = new Stetic.TextBox (6);
			textbox.Show ();
			Add (textbox);

			textbox.Changed += Text_Changed;
		}

		public void Text_Changed (object obj, EventArgs args)
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		public string Value {
			get {
				return textbox.Text;
			}
			set {
				textbox.Text = value;
			}
		}

		public event EventHandler Changed;
	}
}

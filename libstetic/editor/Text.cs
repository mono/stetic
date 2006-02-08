using System;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class Text : Translatable {

		Stetic.TextBox textbox;

		public override void Initialize (PropertyDescriptor prop)
		{
			base.Initialize (prop);
			
			textbox = new Stetic.TextBox (6);
			textbox.Show ();
			Add (textbox);

			textbox.Changed += Text_Changed;
		}
		
		protected override void CheckType (PropertyDescriptor prop)
		{
			if (prop.PropertyType != typeof(string))
				throw new ApplicationException ("Text editor does not support editing values of type " + prop.PropertyType);
				
		}

		public void Text_Changed (object obj, EventArgs args)
		{
			OnValueChanged ();
		}

		public override object Value {
			get {
				return textbox.Text;
			}
			set {
				textbox.Text = (string) value;
			}
		}
	}
}

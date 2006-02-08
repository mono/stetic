using System;

namespace Stetic.Editor {

	[PropertyEditor ("Color", "ColorSet")]
	public class Color : Gtk.ColorButton, IPropertyEditor
	{
		public void Initialize (PropertyDescriptor descriptor)
		{
			if (descriptor.PropertyType != typeof(Gdk.Color))
				throw new ApplicationException ("Color editor does not support editing values of type " + descriptor.PropertyType);
		}
		
		public void AttachObject (object obj)
		{
		}
		
		public object Value { 
			get { return Color; } 
			set { Color = (Gdk.Color) value; }
		}
		
		protected override void OnColorSet ()
		{
			base.OnColorSet ();
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		public event EventHandler ValueChanged;
	}
}

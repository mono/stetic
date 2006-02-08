using System;

namespace Stetic.Editor {

	[PropertyEditor ("Active", "Toggled")]
	public class Boolean : Gtk.CheckButton, IPropertyEditor 
	{
		public void Initialize (PropertyDescriptor descriptor)
		{
			if (descriptor.PropertyType != typeof(bool))
				throw new ApplicationException ("Boolean editor does not support editing values of type " + descriptor.PropertyType);
		}
		
		public void AttachObject (object obj)
		{
		}
		
		public object Value { 
			get { return Active; } 
			set { Active = (bool) value; }
		}
		
		protected override void OnToggled ()
		{
			base.OnToggled ();
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		public event EventHandler ValueChanged;
	}
}

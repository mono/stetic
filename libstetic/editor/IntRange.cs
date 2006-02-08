using System;

namespace Stetic.Editor {

	public class IntRange : Gtk.SpinButton, IPropertyEditor {

		Type propType;
		
		public IntRange () : base (0, 0, 1.0)
		{
		}
		
		public void Initialize (PropertyDescriptor prop)
		{
			propType = prop.PropertyType;
			
			double min, max;
			
			switch (Type.GetTypeCode (propType)) {
				case TypeCode.Int16:
					min = (double) Int16.MinValue;
					max = (double) Int16.MaxValue;
					break;
				case TypeCode.UInt16:
					min = (double) UInt16.MinValue;
					max = (double) UInt16.MaxValue;
					break;
				case TypeCode.Int32:
					min = (double) Int32.MinValue;
					max = (double) Int32.MaxValue;
					break;
				case TypeCode.UInt32:
					min = (double) UInt32.MinValue;
					max = (double) UInt32.MaxValue;
					break;
				case TypeCode.Int64:
					min = (double) Int64.MinValue;
					max = (double) Int64.MaxValue;
					break;
				case TypeCode.UInt64:
					min = (double) UInt64.MinValue;
					max = (double) UInt64.MaxValue;
					break;
				case TypeCode.Byte:
					min = (double) Byte.MinValue;
					max = (double) Byte.MaxValue;
					break;
				case TypeCode.SByte:
					min = (double) SByte.MinValue;
					max = (double) SByte.MaxValue;
					break;
				default:
					throw new ApplicationException ("IntRange editor does not support editing values of type " + prop.PropertyType);
			}

			if (prop.Minimum != null)
				min = (double) Convert.ChangeType (prop.Minimum, typeof(double));
			if (prop.Maximum != null)
				max = (double) Convert.ChangeType (prop.Maximum, typeof(double));
			
			SetRange (min, max);
		}
		
		public void AttachObject (object ob)
		{
		}
		
		object IPropertyEditor.Value {
			get { return Convert.ChangeType (base.Value, propType); }
			set { base.Value = (double) Convert.ChangeType (value, typeof(double)); }
		}
	}
}

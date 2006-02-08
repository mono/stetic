using System;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class StringArray : Text {

		protected override void CheckType (PropertyDescriptor prop)
		{
			if (prop.PropertyType != typeof(string[]))
				throw new ApplicationException ("StringArray editor does not support editing values of type " + prop.PropertyType);
				
		}
		
		public override object Value {
			get {
				return ((string)base.Value).Split ('\n');
			}
			set {
				base.Value = string.Join ("\n", (string[]) value);
			}
		}
	}
}

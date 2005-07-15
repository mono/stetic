using System;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class StringArray : Text {

		public StringArray (PropertyDescriptor prop, object obj) : base (prop, obj) {}

		public new string[] Value {
			get {
				return base.Value.Split ('\n');
			}
			set {
				base.Value = string.Join ("\n", value);
			}
		}
	}
}

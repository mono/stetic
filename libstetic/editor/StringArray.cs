using System;
using System.Reflection;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class StringArray : Text {

		public StringArray (PropertyInfo info) : base (info) {}

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

using System;

namespace Stetic.Editor {

	[PropertyEditor ("StockId", "Changed")]
	public class StockItem : Image {

		public StockItem () : base (true, false) { }
	}
}

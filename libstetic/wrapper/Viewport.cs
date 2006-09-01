using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Viewport : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			Unselectable = true;
		}
	}
}

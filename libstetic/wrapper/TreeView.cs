
using System;

namespace Stetic.Wrapper
{
	public class TreeView: Container
	{
		protected override bool AllowPlaceholders {
			get {
				return false;
			}
		}
	}
}

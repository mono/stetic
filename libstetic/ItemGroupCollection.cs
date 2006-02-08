
using System;
using System.Collections;

namespace Stetic
{
	public class ItemGroupCollection: CollectionBase
	{
		public void Add (ItemGroup group)
		{
			List.Add (group);
		}
		
		public ItemGroup this [int n]
		{
			get {
				return (ItemGroup) List [n];
			}
		}
		
		public ItemGroup this [string name]
		{
			get {
				for (int n=0; n<List.Count; n++) {
					if (((ItemGroup) List [n]).Name == name)
						return (ItemGroup) List [n];
				}
				return null;
			}
		}
	}
}

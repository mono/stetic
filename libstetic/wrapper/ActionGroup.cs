
using System;
using System.Collections;

namespace Stetic.Wrapper
{
	public class ActionGroup
	{
		string name;
		ActionCollection actions = new ActionCollection ();
		
		public ActionGroup ()
		{
		}
		
		public ActionCollection Actions {
			get { return actions; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
	}
	
	public class ActionGroupCollection: CollectionBase
	{
		public void Add (ActionGroup group)
		{
			List.Add (group);
		}
		
		public ActionGroup this [int n] {
			get { return (ActionGroup) List [n]; }
		}
	}
}

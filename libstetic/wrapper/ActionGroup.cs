
using System;
using System.Xml;
using System.Collections;

namespace Stetic.Wrapper
{
	public class ActionGroup
	{
		string name;
		ActionCollection actions;
		
		public ActionGroup ()
		{
			actions = new ActionCollection (this);
		}
		
		public ActionCollection Actions {
			get { return actions; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public Action GetAction (string name)
		{
			foreach (Action ac in actions)
				if (ac.GtkAction.Name == "name")
					return ac;
			return null;
		}
		
		public XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement group = doc.CreateElement ("action-group");
			group.SetAttribute ("name", name);
			foreach (Action ac in actions)
				group.AppendChild (ac.Write (doc, format));
			return group;
		}
		
		public void Read (IProject project, XmlElement elem)
		{
			name = elem.GetAttribute ("name");
			foreach (XmlElement child in elem.SelectNodes ("action")) {
				Action ac = new Action ();
				ac.Read (project, child);
				actions.Add (ac);
			}
		}
		
		internal void NotifyActionAdded (Action ac)
		{
			ac.SetActionGroup (this);
			if (ActionAdded != null)
				ActionAdded (this, new ActionEventArgs (ac));
		}
		
		internal void NotifyActionRemoved (Action ac)
		{
			ac.SetActionGroup (null);
			if (ActionRemoved != null)
				ActionRemoved (this, new ActionEventArgs (ac));
		}
		
		public event ActionEventHandler ActionAdded;
		public event ActionEventHandler ActionRemoved;
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
	
	public delegate void ActionEventHandler (object sender, ActionEventArgs args);
	
	public class ActionEventArgs: EventArgs
	{
		readonly Action action;
		
		public ActionEventArgs (Action ac)
		{
			action = ac;
		}
		
		public Action Action {
			get { return action; }
		}
	}
}


using System;
using System.Collections;

namespace Stetic.Wrapper
{
	public class Action: ObjectWrapper
	{
		ActionType type;
		bool drawAsRadio;
		int radioValue;
		bool active;
		
		public enum ActionType {
			Action,
			Toggle,
			Radio
		}
		
		public Gtk.Action GtkAction {
			get { return (Gtk.Action) Wrapped; }
		}
		
		public ActionType Type {
			get { return type; }
			set { type = value; }
		}
		
		public bool DrawAsRadio {
			get { return drawAsRadio; }
			set { drawAsRadio = value; }
		}
		
		public int Value {
			get { return radioValue; }
			set { radioValue = value; }
		}
		
		public bool Active {
			get { return active; }
			set { active = value; }
		}
		
		public event EventHandler Activated;
		public event EventHandler Toggled;
		public event Gtk.ChangedHandler Changed;
	}
	
	public class ActionCollection: CollectionBase
	{
		public void Add (Action group)
		{
			List.Add (group);
		}
		
		public Action this [int n] {
			get { return (Action) List [n]; }
		}
	}
}

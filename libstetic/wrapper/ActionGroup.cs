
using System;
using System.CodeDom;
using System.Xml;
using System.Collections;

namespace Stetic.Wrapper
{
	public class ActionGroup
	{
		string name;
		ActionCollection actions;
		
		public event ActionEventHandler ActionAdded;
		public event ActionEventHandler ActionRemoved;
		public event EventHandler Changed;
		public event SignalEventHandler SignalAdded;
		public event SignalEventHandler SignalRemoved;
		public event SignalChangedEventHandler SignalChanged;
		
		public ActionGroup ()
		{
			actions = new ActionCollection (this);
		}
		
		public ActionGroup (string name): this ()
		{
			this.name = name;
		}
		
		public ActionCollection Actions {
			get { return actions; }
		}
		
		public string Name {
			get { return name; }
			set { 
				name = value;
				NotifyChanged ();
			}
		}
		
		public Action GetAction (string name)
		{
			foreach (Action ac in actions)
				if (ac.Name == name)
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
		
		internal CodeExpression GenerateObjectCreation (GeneratorContext ctx)
		{
			return new CodeObjectCreateExpression (
				typeof(Gtk.ActionGroup),
				new CodePrimitiveExpression (Name)
			);
		}
		
		internal void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression (varName);
			foreach (Action action in Actions) {
				// Create the action
				string acVar = ctx.NewId ();
				CodeVariableDeclarationStatement uidec = new CodeVariableDeclarationStatement (
					typeof (Gtk.Action),
					acVar,
					action.GenerateObjectCreation (ctx)
				);
				ctx.Statements.Add (uidec);
				action.GenerateBuildCode (ctx, acVar);
				ctx.Statements.Add (
					new CodeMethodInvokeExpression (
						var,
						"Add",
						new CodeVariableReferenceExpression (acVar)
					)
				);
			}
		}
		
		internal void NotifyActionAdded (Action ac)
		{
			ac.SetActionGroup (this);
			ac.SignalAdded += OnSignalAdded;
			ac.SignalRemoved += OnSignalRemoved;
			ac.SignalChanged += OnSignalChanged;
			
			if (ActionAdded != null)
				ActionAdded (this, new ActionEventArgs (ac));
		}
		
		internal void NotifyActionRemoved (Action ac)
		{
			ac.SetActionGroup (null);
			ac.SignalAdded -= OnSignalAdded;
			ac.SignalRemoved -= OnSignalRemoved;
			ac.SignalChanged -= OnSignalChanged;

			if (ActionRemoved != null)
				ActionRemoved (this, new ActionEventArgs (ac));
		}
		
		void NotifyChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		void OnSignalAdded (object s, SignalEventArgs args)
		{
			if (SignalAdded != null)
				SignalAdded (this, args);
		}
		
		void OnSignalRemoved (object s, SignalEventArgs args)
		{
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}
		
		void OnSignalChanged (object s, SignalChangedEventArgs args)
		{
			if (SignalChanged != null)
				SignalChanged (this, args);
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
		

		protected override void OnInsertComplete (int index, object val)
		{
			NotifyGroupAdded ((ActionGroup) val);
		}
		
		protected override void OnRemoveComplete (int index, object val)
		{
			NotifyGroupRemoved ((ActionGroup)val);
		}
		
		protected override void OnSetComplete (int index, object oldv, object newv)
		{
			NotifyGroupRemoved ((ActionGroup) oldv);
			NotifyGroupAdded ((ActionGroup) newv);
		}
		
		void NotifyGroupAdded (ActionGroup grp)
		{
			grp.Changed += OnGroupChanged;
			if (ActionGroupAdded != null)
				ActionGroupAdded (this, new ActionGroupEventArgs (grp));
		}
		
		void NotifyGroupRemoved (ActionGroup grp)
		{
			grp.Changed -= OnGroupChanged;
			if (ActionGroupRemoved != null)
				ActionGroupRemoved (this, new ActionGroupEventArgs (grp));
		}
		
		void OnGroupChanged (object s, EventArgs a)
		{
			if (ActionGroupChanged != null)
				ActionGroupChanged (this, new ActionGroupEventArgs ((ActionGroup)s));
		}
		
		public event ActionGroupEventHandler ActionGroupAdded;
		public event ActionGroupEventHandler ActionGroupRemoved;
		public event ActionGroupEventHandler ActionGroupChanged;
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
	
	public delegate void ActionGroupEventHandler (object sender, ActionGroupEventArgs args);
	
	public class ActionGroupEventArgs: EventArgs
	{
		readonly ActionGroup action;
		
		public ActionGroupEventArgs (ActionGroup ac)
		{
			action = ac;
		}
		
		public ActionGroup ActionGroup {
			get { return action; }
		}
	}
}

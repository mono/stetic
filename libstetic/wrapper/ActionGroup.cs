
using System;
using System.CodeDom;
using System.Xml;
using System.Collections;
using Stetic.Undo;

namespace Stetic.Wrapper
{
	public sealed class ActionGroup: MarshalByRefObject, IDisposable
	{
		string name;
		ActionCollection actions;
		IObjectFrontend frontend;
		ObjectWrapper owner;
		
		// This id is used by the undo methods to identify an object.
		string undoId = WidgetUtils.GetUndoId ();
		
		public event ActionEventHandler ActionAdded;
		public event ActionEventHandler ActionRemoved;
		public event ActionEventHandler ActionChanged;
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
		
		public void Dispose ()
		{
			foreach (Action a in actions)
				a.Dispose ();
		}
		
		internal string UndoId {
			get { return undoId; }
			set { undoId = value; }
		}
		
		public IObjectFrontend Frontend {
			get { return frontend; }
			set {
				if (frontend != null)
					frontend.Dispose ();
				frontend = value;
			}
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
		
		internal string GetValidName (Action reqAction, string name)
		{
			int max = 0;
			bool found = false;
			foreach (Action ac in Actions) {
				if (ac == reqAction)
					continue;
					
				string bname;
				int index;
				WidgetUtils.ParseWidgetName (ac.Name, out bname, out index);
				
				if (name == ac.Name)
					found = true;
				if (name == bname && index > max)
					max = index;
			}
			if (found)
				return name + (max+1);
			else
				return name;
		}
		
		public XmlElement Write (ObjectWriter writer)
		{
			XmlElement group = writer.XmlDocument.CreateElement ("action-group");
			group.SetAttribute ("name", name);
			if (writer.CreateUndoInfo)
				group.SetAttribute ("undoId", undoId);
			foreach (Action ac in actions)
				group.AppendChild (ac.Write (writer));
			return group;
		}
		
		public void Read (IProject project, XmlElement elem)
		{
			name = elem.GetAttribute ("name");
			undoId = elem.GetAttribute ("undoId");
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
				Type atype;
				if (action.Type == Action.ActionType.Action)
					atype = typeof (Gtk.Action);
				else if (action.Type == Action.ActionType.Radio)
					atype = typeof(Gtk.RadioAction);
				else
					atype = typeof(Gtk.ToggleAction);
					
				CodeVariableDeclarationStatement uidec = new CodeVariableDeclarationStatement (
					atype,
					acVar,
					action.GenerateObjectCreation (ctx)
				);
				ctx.Statements.Add (uidec);
				ctx.GenerateBuildCode (action, acVar);
				ctx.Statements.Add (
					new CodeMethodInvokeExpression (
						var,
						"Add",
						new CodeVariableReferenceExpression (acVar),
						new CodePrimitiveExpression (action.Accelerator)
					)
				);
			}
		}
		
		internal void SetOwner (ObjectWrapper owner)
		{
			this.owner = owner;
		}
		
		internal UndoManager GetUndoManager ()
		{
			if (owner != null)
				return owner.UndoManager;
			else
				return null;
		}
		
		internal void NotifyActionAdded (Action ac)
		{
			ac.SetActionGroup (this);
			ac.ObjectChanged += OnActionChanged;
			ac.SignalAdded += OnSignalAdded;
			ac.SignalRemoved += OnSignalRemoved;
			ac.SignalChanged += OnSignalChanged;
			
			ac.UpdateNameIndex ();
			
			NotifyChanged ();
			
			if (ActionAdded != null)
				ActionAdded (this, new ActionEventArgs (ac));
		}
		
		internal void NotifyActionRemoved (Action ac)
		{
			ac.SetActionGroup (null);
			ac.ObjectChanged -= OnActionChanged;
			ac.SignalAdded -= OnSignalAdded;
			ac.SignalRemoved -= OnSignalRemoved;
			ac.SignalChanged -= OnSignalChanged;

			NotifyChanged ();
			
			if (ActionRemoved != null)
				ActionRemoved (this, new ActionEventArgs (ac));
		}
		
		void NotifyChanged ()
		{
			if (frontend != null)
				frontend.NotifyChanged ();
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		void OnActionChanged (object s, ObjectWrapperEventArgs args)
		{
			NotifyChanged ();
			if (ActionChanged != null)
				ActionChanged (this, new ActionEventArgs ((Action) args.Wrapper));
		}
		
		void OnSignalAdded (object s, SignalEventArgs args)
		{
			NotifyChanged ();
			if (SignalAdded != null)
				SignalAdded (this, args);
		}
		
		void OnSignalRemoved (object s, SignalEventArgs args)
		{
			NotifyChanged ();
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}
		
		void OnSignalChanged (object s, SignalChangedEventArgs args)
		{
			NotifyChanged ();
			if (SignalChanged != null)
				SignalChanged (this, args);
		}
	}
	
	public class ActionGroupCollection: CollectionBase
	{
		ActionGroup[] toClear;
		ObjectWrapper owner;
		
		internal void SetOwner (ObjectWrapper owner)
		{
			this.owner = owner;
		}
		
		public void Add (ActionGroup group)
		{
			List.Add (group);
		}
		
		public void Insert (int n, ActionGroup group)
		{
			List.Insert (n, group);
		}
		
		public ActionGroup this [int n] {
			get { return (ActionGroup) List [n]; }
		}
		
		public ActionGroup this [string name] {
			get {
				foreach (ActionGroup grp in List)
					if (grp.Name == name)
						return grp;
				return null;
			}
		}
		
		DiffGenerator GetDiffGenerator (IProject prj)
		{
			DiffGenerator gen = new DiffGenerator ();
			gen.CurrentStatusAdaptor = new ActionDiffAdaptor (prj);
			XmlDiffAdaptor xad = new XmlDiffAdaptor ();
			xad.ChildElementName = "action-group";
			xad.ProcessProperties = false;
			xad.ChildAdaptor = new XmlDiffAdaptor ();
			xad.ChildAdaptor.ChildElementName = "action";
			gen.NewStatusAdaptor = xad;
			return gen;
		}
		
		internal ObjectDiff GetDiff (IProject prj, XmlElement elem)
		{
			return GetDiffGenerator (prj).GetDiff (this, elem);
		}
		
		internal void ApplyDiff (IProject prj, ObjectDiff diff)
		{
			GetDiffGenerator (prj).ApplyDiff (this, diff);
		}
		
		public int IndexOf (ActionGroup group)
		{
			return List.IndexOf (group);
		}
		
		public void Remove (ActionGroup group)
		{
			List.Remove (group);
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
		
		protected override void OnClear ()
		{
			toClear = new ActionGroup [Count];
			List.CopyTo (toClear, 0);
		}
		
		protected override void OnClearComplete ()
		{
			foreach (ActionGroup a in toClear)
				NotifyGroupRemoved (a);
			toClear = null;
		}
		
		void NotifyGroupAdded (ActionGroup grp)
		{
			grp.SetOwner (owner);
			grp.Changed += OnGroupChanged;
			if (ActionGroupAdded != null)
				ActionGroupAdded (this, new ActionGroupEventArgs (grp));
		}
		
		void NotifyGroupRemoved (ActionGroup grp)
		{
			grp.SetOwner (null);
			grp.Changed -= OnGroupChanged;
			if (ActionGroupRemoved != null)
				ActionGroupRemoved (this, new ActionGroupEventArgs (grp));
		}
		
		void OnGroupChanged (object s, EventArgs a)
		{
			if (ActionGroupChanged != null)
				ActionGroupChanged (this, new ActionGroupEventArgs ((ActionGroup)s));
		}
		
		public ActionGroup[] ToArray ()
		{
			ActionGroup[] groups = new ActionGroup [Count];
			List.CopyTo (groups, 0);
			return groups;
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

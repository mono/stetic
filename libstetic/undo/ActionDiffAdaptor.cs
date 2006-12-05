
using System;
using System.Xml;
using System.Collections;
using Stetic.Wrapper;

namespace Stetic.Undo
{
	class ActionDiffAdaptor: IDiffAdaptor
	{
		IProject project;
		
		public ActionDiffAdaptor (IProject project)
		{
			this.project = project;
		}
		
		public IEnumerable GetChildren (object parent)
		{
			if (parent is Action)
				return Type.EmptyTypes;
			if (parent is ActionGroup)
				return ((ActionGroup)parent).Actions;
			if (parent is ActionGroupCollection)
				return (ActionGroupCollection) parent;

			throw new NotImplementedException ();
		}
		
		public string GetUndoId (object childObject)
		{
			if (childObject is ActionGroup)
				return ((ActionGroup)childObject).UndoId;
			if (childObject is Action)
				return ((Action)childObject).UndoId;

			throw new NotImplementedException ();
		}
		
		public object FindChild (object parent, string undoId)
		{
			foreach (object ob in GetChildren (parent))
				if (GetUndoId (ob) == undoId)
					return ob;
			return null;
		}
		
		public void RemoveChild (object parent, string undoId)
		{
			object child = FindChild (parent, undoId);
			if (parent is ActionGroup) {
				((ActionGroup)parent).Actions.Remove ((Action)child);
			} else if (parent is ActionGroupCollection) {
				((ActionGroupCollection)parent).Remove ((ActionGroup)child);
			} else
				throw new NotImplementedException ();
		}
		
		public void AddChild (object parent, XmlElement node, string insertAfter)
		{
			object data = DeserializeChild (node);
			if (parent is ActionGroup) {
				ActionGroup group = (ActionGroup) parent;
				if (insertAfter == null)
					group.Actions.Insert (0, (Action) data);
				else {
					for (int n=0; n<group.Actions.Count; n++) {
						if (group.Actions [n].UndoId == insertAfter) {
							group.Actions.Insert (n+1, (Action) data);
							return;
						}
					}
					group.Actions.Add ((Action) data);
				}
			}
			if (parent is ActionGroupCollection) {
				ActionGroupCollection col = (ActionGroupCollection) parent;
				if (insertAfter == null)
					col.Insert (0, (ActionGroup) data);
				else {
					for (int n=0; n<col.Count; n++) {
						if (col [n].UndoId == insertAfter) {
							col.Insert (n+1, (ActionGroup) data);
							return;
						}
					}
					col.Add ((ActionGroup) data);
				}
			}
		}
		
		public IEnumerable GetProperties (object obj)
		{
			Action action = obj as Action;
			if (action != null) {
				foreach (ItemGroup iset in action.ClassDescriptor.ItemGroups) {
					foreach (ItemDescriptor it in iset) {
						PropertyDescriptor prop = it as PropertyDescriptor;
						
						if (!prop.VisibleFor (action.Wrapped) || !prop.CanWrite || prop.Name == "Name")
							continue;

						object value = prop.GetValue (action.Wrapped);
						
						// If the property has its default value, we don't need to check it
						if (value == null || (prop.HasDefault && prop.IsDefaultValue (value)))
							continue;
						
						yield return it;
					}
				}
			}
			else
				yield break;
		}
		
		public XmlElement SerializeChild (object child)
		{
			XmlDocument doc = new XmlDocument ();
			ObjectWriter ow = new ObjectWriter (doc, FileFormat.Native);
			
			if (child is Action) {
				return ((Action)child).Write (ow);
			} else if (child is ActionGroup) {
				return ((ActionGroup)child).Write (ow);
			}
			throw new NotImplementedException ();
		}
		
		public object DeserializeChild (XmlElement data)
		{
			if (data.LocalName == "action") {
				Action ac = new Action ();
				ac.Read (project, data);
				return ac;
			} else if (data.LocalName == "action-group") {
				ActionGroup ac = new ActionGroup ();
				ac.Read (project, data);
				return ac;
			}
			throw new NotImplementedException ();
		}
		
		public IDiffAdaptor GetChildAdaptor (object child)
		{
			return this;
		}
		
		public object GetPropertyByName (object obj, string name)
		{
			return ((Action)obj).ClassDescriptor [name];
		}
		
		public string GetPropertyName (object property)
		{
			return ((PropertyDescriptor)property).Name;
		}
		
		public string GetPropertyValue (object obj, object property)
		{
			PropertyDescriptor prop = (PropertyDescriptor) property;
			object val = prop.GetValue (((Action)obj).Wrapped);
			return prop.ValueToString (val);
		}
		
		public void SetPropertyValue (object obj, string name, string value)
		{
			PropertyDescriptor prop = (PropertyDescriptor) GetPropertyByName (obj, name);
			if (prop == null)
				throw new InvalidOperationException ("Property '" + name + "' not found in object of type: " + obj.GetType ());
			prop.SetValue (((Action)obj).Wrapped, prop.StringToValue (value));
		}

		public void ResetPropertyValue (object obj, string name)
		{
			PropertyDescriptor prop = (PropertyDescriptor) GetPropertyByName (obj, name);
			prop.ResetValue (((Action)obj).Wrapped);
		}
		
		public IEnumerable GetSignals (object obj)
		{
			if (obj is Action) {
				foreach (Signal s in ((Action)obj).Signals)
					yield return s;
			}
			else
				yield break;
		}
		
		public object GetSignal (object obj, string name, string handler)
		{
			foreach (Signal s in ((Action)obj).Signals) {
				if (s.SignalDescriptor.Name == name && s.Handler == handler)
					return s;
			}
			return null;
		}
		
		public void GetSignalInfo (object signal, out string name, out string handler)
		{
			Signal s = (Signal) signal;
			name = s.SignalDescriptor.Name;
			handler = s.Handler;
		}
		
		public void AddSignal (object obj, string name, string handler)
		{
			SignalDescriptor sd = (SignalDescriptor) ((Action)obj).ClassDescriptor.SignalGroups.GetItem (name);
			Signal sig = new Signal (sd);
			sig.Handler = handler;
			((Action)obj).Signals.Add (sig);
		}
		
		public void RemoveSignal (object obj, string name, string handler)
		{
			foreach (Signal sig in ((Action)obj).Signals) {
				if (sig.SignalDescriptor.Name == name && sig.Handler == handler) {
					((Action)obj).Signals.Remove (sig);
					return;
				}
			}
		}
	}
}

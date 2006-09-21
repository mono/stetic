
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Stetic
{
	[Serializable]
	public class ActionGroupComponent: Component
	{
		public ActionGroupComponent (Application app, object backend, string name): base (app, backend, name, app.GetComponentType ("Gtk.ActionGroup"))
		{
		}
		
		public override string Name {
			get {
				if (name == null)
					name = ((Wrapper.ActionGroup)backend).Name;
				return name;
			}
			set {
				name = value;
				((Wrapper.ActionGroup)backend).Name = value;
			}
		}
		
		protected override void OnChanged ()
		{
			name = null;
			base.OnChanged ();
		}
		
		public ActionComponent[] GetActions ()
		{
			Wrapper.ActionCollection acts = ((Wrapper.ActionGroup)backend).Actions;
			ActionComponent[] comps = new ActionComponent [acts.Count];
			
			for (int n=0; n<acts.Count; n++)
				comps [n] = (ActionComponent) app.GetComponent (acts[n], null, null);
				
			return comps;
		}
		
		public override Component[] GetChildren ()
		{
			return GetActions ();
		}
	}
}

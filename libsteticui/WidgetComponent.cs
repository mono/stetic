
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Stetic
{
	public class WidgetComponent: Component
	{
		static WidgetComponent placeholder;
		
		public WidgetComponent (Application app, object backend, string name, ComponentType type): base (app, backend, name, type)
		{
		}

		public override string Name {
			get {
				if (name == null)
					UpdateComponentInfo ();
				return name;
			}
			set {
				name = value;
				if (app != null)
					app.Backend.RenameWidget ((Wrapper.Widget)backend, value);
			}
		}
		
		internal void UpdateName (string name)
		{
			this.name = name;
		}
		
		public override Component[] GetChildren ()
		{
			if (app == null)
				return new Component [0];
			ArrayList wws = app.Backend.GetWidgetChildren ((Wrapper.Widget)backend);
			if (wws == null)
				return new Component [0];
			Component[] children = new Component [wws.Count];
			for (int n=0; n<wws.Count; n++)
				children [n] = app.GetComponent (wws[n], null, null);
			return children;
		}
		
		protected override void OnGetClipboardStatus ()
		{
			if (app != null)
				app.Backend.GetClipboardOperations (backend, out canCut, out canCopy, out canPaste);
		}
		
		void UpdateComponentInfo ()
		{
			if (app == null)
				return;
			string typeName;
			app.Backend.GetComponentInfo (backend, out name, out typeName);
			type = app.GetComponentType (typeName);
		}
		
		public override ComponentType Type {
			get {
				if (type == null)
					UpdateComponentInfo ();
				return type;
			}
		}
		
		public ActionGroupComponent[] GetActionGroups ()
		{
			if (app == null)
				return new ActionGroupComponent [0];
			
			Wrapper.ActionGroup[] groups = app.Backend.GetActionGroups ((Wrapper.Widget)backend);
			ActionGroupComponent[] comps = new ActionGroupComponent [groups.Length];
			for (int n=0; n<groups.Length; n++)
				comps [n] = (ActionGroupComponent) app.GetComponent (groups[n], null, null);
				
			return comps;
		}
		
		public bool IsWindow {
			get { return backend is Wrapper.Window; }
		}
		
		internal static WidgetComponent Placeholder {
			get {
				if (placeholder == null) {
					placeholder = new WidgetComponent (null, null, "", ComponentType.Unknown);
				}
				return placeholder;
			}
		}
	}
	
	[Serializable]
	public class ObjectBindInfo
	{
		string typeName;
		string name;
		
		public ObjectBindInfo (string typeName, string name)
		{
			this.typeName = typeName;
			this.name = name;
		}
		
		public string TypeName {
			get { return typeName; }
		}
		
		public string Name {
			get { return name; }
		}
	}
}

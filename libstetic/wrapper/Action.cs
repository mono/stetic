
using System;
using System.Text;
using System.Xml;
using System.Collections;

namespace Stetic.Wrapper
{
	public class Action: ObjectWrapper
	{
		ActionType type;
		bool drawAsRadio;
		int radioValue;
		bool active;
		string name;
		ActionGroup group;
		
		public enum ActionType {
			Action,
			Toggle,
			Radio
		}
		
		public event EventHandler Activated;
		public event EventHandler Toggled;
		public event Gtk.ChangedHandler Changed;
		
		public Gtk.Action GtkAction {
			get { return (Gtk.Action) Wrapped; }
		}
		
		public string Name {
			get {
				if (name == null || name.Length == 0)
					return GetIdentifier (GtkAction.Label);
				return name;
			}
			set { name = value; }
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
			set { 
				active = value;
				if (Activated != null)
					Activated (this, EventArgs.Empty);
				if (Toggled != null)
					Toggled (this, EventArgs.Empty);
				if (Changed != null)
					Changed (this, new Gtk.ChangedArgs ());
			}
		}
		
		public ActionGroup ActionGroup {
			get { return group; }
		}
		
		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = doc.CreateElement ("action");
			WidgetUtils.GetProps (this, elem);
			WidgetUtils.GetSignals (this, elem);
			return elem;
		}
		
		public void Read (IProject project, XmlElement elem)
		{
			string name = elem.GetAttribute ("name");
			Gtk.Action ac = new Gtk.Action (name, "");
			
			ClassDescriptor klass = Registry.LookupClassByName ("Gtk.Action");
			ObjectWrapper.Bind (project, klass, this, ac, true);
			
			WidgetUtils.ReadProperties (klass, this, ac, elem);
			WidgetUtils.ReadSignals (klass, this, elem);
		}
		
		public Action Clone ()
		{
			Action a = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("", ""));
			a.CopyFrom (this);
			return a;
		}
		
		public void CopyFrom (Action action)
		{
			type = action.type;
			drawAsRadio = action.drawAsRadio;
			radioValue = action.radioValue;
			active = action.active;
			name = action.name;
			GtkAction.HideIfEmpty = action.GtkAction.HideIfEmpty;
			GtkAction.IsImportant = action.GtkAction.IsImportant;
			GtkAction.Label = action.GtkAction.Label;
			GtkAction.Sensitive = action.GtkAction.Sensitive;
			GtkAction.ShortLabel = action.GtkAction.ShortLabel;
			GtkAction.StockId = action.GtkAction.StockId;
			GtkAction.Tooltip = action.GtkAction.Tooltip;
			GtkAction.Visible = action.GtkAction.Visible;
			GtkAction.VisibleHorizontal = action.GtkAction.VisibleHorizontal;
			GtkAction.VisibleVertical = action.GtkAction.VisibleVertical;
			NotifyChanged ();
		}
		
		internal void SetActionGroup (ActionGroup g)
		{
			group = g;
		}
		
		string GetIdentifier (string name)
		{
			StringBuilder sb = new StringBuilder ();
			
			bool wstart = false;
			foreach (char c in name) {
				if ((c == '-' || c == ' ' || !char.IsLetterOrDigit (c)) && c != '_') {
					wstart = true;
					continue;
				}
				if (wstart) {
					sb.Append (char.ToUpper (c));
					wstart = false;
				} else
					sb.Append (c);
			}
			return sb.ToString ();
		}
	}
	
	public class ActionCollection: CollectionBase
	{
		ActionGroup group;
		
		public ActionCollection ()
		{
		}
		
		internal ActionCollection (ActionGroup group)
		{
			this.group = group;
		}
		
		public void Add (Action group)
		{
			List.Add (group);
		}
		
		public Action this [int n] {
			get { return (Action) List [n]; }
		}
		
		public void Remove (Action action)
		{
			List.Remove (action);
		}
		
		public bool Contains (Action action)
		{
			return List.Contains (action);
		}
		
		public void CopyTo (Action[] array, int index)
		{
			List.CopyTo (array, index);
		}

		protected override void OnInsertComplete (int index, object val)
		{
			group.NotifyActionAdded ((Action) val);
		}
		
		protected override void OnRemoveComplete (int index, object val)
		{
			group.NotifyActionRemoved ((Action)val);
		}
		
		protected override void OnSetComplete (int index, object oldv, object newv)
		{
			group.NotifyActionRemoved ((Action) oldv);
			group.NotifyActionAdded ((Action) newv);
		}
	}
}


using System;
using System.Text;
using System.Xml;
using System.CodeDom;
using System.Collections;

namespace Stetic.Wrapper
{
	public sealed class Action: Stetic.Wrapper.Object
	{
		ActionType type;
		bool drawAsRadio;
		int radioValue;
		bool active;
		string name;
		string accelerator;
		ActionGroup group;
		
		string oldDefaultName;
		string nameRoot;

		public enum ActionType {
			Action,
			Toggle,
			Radio
		}
		
		public event EventHandler Activated;
		public event EventHandler Toggled;
		public event Gtk.ChangedHandler Changed;
		public event EventHandler Deleted;
		
		public Gtk.Action GtkAction {
			get { return (Gtk.Action) Wrapped; }
		}
		
		public string Name {
			get {
				if (name == null || name.Length == 0) {
					name = nameRoot = oldDefaultName = GetDefaultName ();
					if (group != null)
						name = group.GetValidName (this, name);
				}
				return name;
			}
			set {
				name = nameRoot = value;
				if (group != null)
					name = group.GetValidName (this, name);
				EmitNotify ("Name");
			}
		}
		
		internal void UpdateNameIndex ()
		{
			// Adds a number to the action name if the current name already
			// exists in the action group.
			
			string vname = group.GetValidName (this, Name);
			if (vname != Name) {
				name = vname;
				EmitNotify ("Name");
			}
		}

		string GetDefaultName ()
		{
			if (GtkAction.Label != null && GtkAction.Label.Length > 0)
				return GetIdentifier (GtkAction.Label);

			if (GtkAction.StockId != null) {
				string s = GtkAction.StockId.Replace ("gtk-", "");
				return GetIdentifier (s.Replace ("gnome-stock-", ""));
			}
			return null;
		}
		
		public ActionType Type {
			get { return type; }
			set { type = value; EmitNotify ("Type"); }
		}
		
		public bool DrawAsRadio {
			get { return drawAsRadio; }
			set { drawAsRadio = value; EmitNotify ("DrawAsRadio"); }
		}
		
		public int Value {
			get { return radioValue; }
			set { radioValue = value; EmitNotify ("Value"); }
		}
		
		public string Accelerator {
			get { return accelerator; }
			set { accelerator = value; EmitNotify ("Accelerator"); }
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
		
		public string MenuLabel {
			get {
				if (GtkAction.Label != null && GtkAction.Label.Length > 0)
					return GtkAction.Label;

				if (GtkAction.StockId == null)
					return "";

				Gtk.StockItem item = Gtk.Stock.Lookup (GtkAction.StockId);
				if (item.Label != null)
					return item.Label;

				return "";
			}
		}
		
		public string ToolLabel {
			get {
				if (GtkAction.ShortLabel != null && GtkAction.ShortLabel.Length > 0)
					return GtkAction.ShortLabel;
				else
					return MenuLabel;
			}
		}
		
		public ActionGroup ActionGroup {
			get { return group; }
		}
		
		public void Delete ()
		{
			if (group != null)
				group.Actions.Remove (this);
			if (Deleted != null)
				Deleted (this, EventArgs.Empty);
			Dispose ();
		}
		
		protected override void EmitNotify (string propertyName)
		{
			if (propertyName == "Label" || propertyName == "StockId") {
				// If the current name is a name generated from label or stockid,
				// we update here the name again
				if (nameRoot == oldDefaultName)
					Name = GetDefaultName ();
				oldDefaultName = GetDefaultName ();
			}
			base.EmitNotify (propertyName);
		}
		
		public override XmlElement Write (ObjectWriter writer)
		{
			XmlElement elem = writer.XmlDocument.CreateElement ("action");
			elem.SetAttribute ("id", Name);
			WidgetUtils.GetProps (this, elem);
			WidgetUtils.GetSignals (this, elem);
			return elem;
		}
		
		public void Read (IProject project, XmlElement elem)
		{
			Gtk.Action ac = new Gtk.Action (name, "");
			
			ClassDescriptor klass = Registry.LookupClassByName ("Gtk.Action");
			ObjectWrapper.Bind (project, klass, this, ac, true);
			
			WidgetUtils.ReadMembers (klass, this, ac, elem);
			name = nameRoot = oldDefaultName = elem.GetAttribute ("id");
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
			
			Signals.Clear ();
			foreach (Signal s in action.Signals)
				Signals.Add (new Signal (s.SignalDescriptor, s.Handler, s.After));
			
			NotifyChanged ();
		}
		
		public Gtk.Widget CreateIcon (Gtk.IconSize size)
		{
			if (GtkAction.StockId == null)
				return null;

			Gdk.Pixbuf px = Project.IconFactory.RenderIcon (Project, GtkAction.StockId, size);
			if (px != null)
				return new Gtk.Image (px);
			else
				return GtkAction.CreateIcon (size);
		}
		
		public Gdk.Pixbuf RenderIcon (Gtk.IconSize size)
		{
			if (GtkAction.StockId == null)
				return null;

			Gdk.Pixbuf px = Project.IconFactory.RenderIcon (Project, GtkAction.StockId, size);
			if (px != null)
				return px;

			Gtk.IconSet iset = Gtk.IconFactory.LookupDefault (GtkAction.StockId);
			if (iset == null)
				iset = Gtk.IconFactory.LookupDefault (Gtk.Stock.MissingImage);
			return iset.RenderIcon (new Gtk.Style (), Gtk.TextDirection.Ltr, Gtk.StateType.Normal, size, null, "");
		}
		
		internal protected override CodeExpression GenerateObjectCreation (GeneratorContext ctx)
		{
			CodeObjectCreateExpression exp = new CodeObjectCreateExpression ();
			
			PropertyDescriptor prop = (PropertyDescriptor) ClassDescriptor ["Name"];
			exp.Parameters.Add (ctx.GenerateValue (prop.GetValue (Wrapped), prop.RuntimePropertyType));
			
			prop = (PropertyDescriptor) ClassDescriptor ["Label"];
			string lab = (string) prop.GetValue (Wrapped);
			if (lab == "") lab = null;
			exp.Parameters.Add (ctx.GenerateValue (lab, prop.RuntimePropertyType, prop.Translatable));
			
			prop = (PropertyDescriptor) ClassDescriptor ["Tooltip"];
			exp.Parameters.Add (ctx.GenerateValue (prop.GetValue (Wrapped), prop.RuntimePropertyType, prop.Translatable));
			
			prop = (PropertyDescriptor) ClassDescriptor ["StockId"];
			exp.Parameters.Add (ctx.GenerateValue (prop.GetValue (Wrapped), prop.RuntimePropertyType, prop.Translatable));
			
			if (type == ActionType.Action)
				exp.CreateType = new CodeTypeReference ("Gtk.Action");
			else if (type == ActionType.Toggle)
				exp.CreateType = new CodeTypeReference ("Gtk.ToggleAction");
			else {
				exp.CreateType = new CodeTypeReference ("Gtk.RadioAction");
				prop = (PropertyDescriptor) ClassDescriptor ["Value"];
				exp.Parameters.Add (ctx.GenerateValue (prop.GetValue (Wrapped), typeof(int)));
			}
			return exp;
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
				if (c == '_' || (int)c > 127)	// No underline, no unicode
					continue;
				if (c == '-' || c == ' ' || !char.IsLetterOrDigit (c)) {
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
	
	[Serializable]
	public class ActionCollection: CollectionBase
	{
		[NonSerialized]
		ActionGroup group;
		
		public ActionCollection ()
		{
		}
		
		internal ActionCollection (ActionGroup group)
		{
			this.group = group;
		}
		
		public void Add (Action action)
		{
			List.Add (action);
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
			if (group != null)
				group.NotifyActionAdded ((Action) val);
		}
		
		protected override void OnRemoveComplete (int index, object val)
		{
			if (group != null)
				group.NotifyActionRemoved ((Action)val);
		}
		
		protected override void OnSetComplete (int index, object oldv, object newv)
		{
			if (group != null) {
				group.NotifyActionRemoved ((Action) oldv);
				group.NotifyActionAdded ((Action) newv);
			}
		}
	}
}

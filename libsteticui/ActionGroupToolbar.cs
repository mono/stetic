
using System;
using System.Collections;
using Stetic.Wrapper;
using Mono.Unix;

namespace Stetic
{
	public class ActionGroupToolbar: Gtk.Toolbar
	{
		Wrapper.ActionGroupCollection actionGroups;
		Gtk.ComboBox combo;
		bool updating;
		ActionGroup currentGroup;
		ArrayList internalButtons = new ArrayList ();
		bool singleGroupMode;
		
		public ActionGroupToolbar (bool singleGroupMode)
		{
			Initialize (null, singleGroupMode);
		}
		
		public ActionGroupToolbar (Wrapper.ActionGroup actionGroup)
		{
			currentGroup = actionGroup;
			Initialize (null, true);
		}
		
		public ActionGroupToolbar (Wrapper.ActionGroupCollection actionGroups)
		{
			Initialize (actionGroups, false);
		}
		
		void Initialize (Wrapper.ActionGroupCollection actionGroups, bool singleGroupMode)
		{
			this.singleGroupMode = singleGroupMode;
			IconSize = Gtk.IconSize.SmallToolbar;
			Orientation = Gtk.Orientation.Horizontal;
			ToolbarStyle = Gtk.ToolbarStyle.BothHoriz;
			
			combo = Gtk.ComboBox.NewText ();
			
			if (!singleGroupMode) {
				combo.Changed += OnActiveChanged;

				Gtk.ToolItem comboItem = new Gtk.ToolItem ();
				Gtk.HBox cbox = new Gtk.HBox ();
				cbox.PackStart (new Gtk.Label (Catalog.GetString ("Action Group:") + " "), false, false, 3);
				cbox.PackStart (combo, true, true, 3);
				comboItem.Add (cbox);
				comboItem.ShowAll ();
				Insert (comboItem, -1);
				internalButtons.Add (comboItem);
				
				Gtk.ToolButton but = new Gtk.ToolButton (Gtk.Stock.Add);
				but.Clicked += OnAddGroup;
				Insert (but, -1);
				internalButtons.Add (but);
				
				but = new Gtk.ToolButton (Gtk.Stock.Remove);
				but.Clicked += OnRemoveGroup;
				Insert (but, -1);
				internalButtons.Add (but);
				
				ActionGroups = actionGroups;
				
				if (actionGroups != null && actionGroups.Count > 0)
					combo.Active = 0;
			} else {
				UpdateActionCommands (null);
			}

			ShowAll ();
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (!singleGroupMode)
				ActionGroups = null;
		}
		
		public Wrapper.ActionGroupCollection ActionGroups {
			get { return actionGroups; }
			set {
				if (singleGroupMode)
					throw new InvalidOperationException ("ActionGroups can't be set in single group mode");

				if (actionGroups != null) {
					actionGroups.ActionGroupAdded -= OnCollectionChanged;
					actionGroups.ActionGroupRemoved -= OnCollectionChanged;
					actionGroups.ActionGroupChanged -= OnCollectionChanged;
				}
				
				this.actionGroups = value;
				
				if (actionGroups != null) {
					actionGroups.ActionGroupAdded += OnCollectionChanged;
					actionGroups.ActionGroupRemoved += OnCollectionChanged;
					actionGroups.ActionGroupChanged += OnCollectionChanged;
				}
				Refresh ();
			}
		}
		
		public void Bind (Editor.ActionGroupEditor agroupEditor)
		{
			ActiveGroupChanged += delegate (object s, Wrapper.ActionGroupEventArgs args) {
				agroupEditor.ActionGroup = args.ActionGroup;
			};
			ActiveGroupCreated += delegate (object s, Wrapper.ActionGroupEventArgs args) {
				agroupEditor.StartEditing ();
			};
			
			agroupEditor.SelectionChanged += delegate (object s, EventArgs args) {
				UpdateActionCommands (agroupEditor.SelectedAction);
			};
			agroupEditor.ActionGroup = ActiveGroup;
		}
		
		public ActionGroup ActiveGroup {
			get {
				return currentGroup;
			}
			set {
				if (singleGroupMode) {
					currentGroup = value;
					UpdateActionCommands (null);
					if (ActiveGroupChanged != null)
						ActiveGroupChanged (this, new ActionGroupEventArgs (ActiveGroup));
				} else {
					int i = actionGroups.IndexOf (value);
					if (i != -1)
						combo.Active = i;
				}
			}
		}
		
		void Refresh ()
		{
			if (singleGroupMode)
				return;

			while (combo.Model.IterNChildren () > 0)
				combo.RemoveText (0);
			if (actionGroups != null) {
				foreach (ActionGroup group in actionGroups)
					combo.AppendText (group.Name);
			}
		}
		
		void OnCollectionChanged (object s, ActionGroupEventArgs args)
		{
			// Avoid firing the selection change event if the selected
			// group is the same after the refresh
			ActionGroup oldGroup = currentGroup;
			updating = true;
			
			int i = combo.Active;
			Refresh ();
			if (actionGroups.Count == 0) {
				combo.Sensitive = false;
				currentGroup = null;
			}
			else {
				combo.Sensitive = true;
				if (i == -1)
					i = 0;
				if (i < actionGroups.Count)
					combo.Active = i;
				else
					combo.Active = actionGroups.Count - 1;
				currentGroup = (ActionGroup) actionGroups [combo.Active];
			}
			updating = false;
			if (currentGroup != oldGroup)
				OnActiveChanged (null, null);
		}
		
		void OnAddGroup (object s, EventArgs args)
		{
			ActionGroup group = new ActionGroup ();
			group.Name = Catalog.GetString ("New Action Group");
			actionGroups.Add (group);
			combo.Active = actionGroups.Count - 1;
			if (ActiveGroupCreated != null)
				ActiveGroupCreated (this, new ActionGroupEventArgs (ActiveGroup));
		}
		
		void OnRemoveGroup (object s, EventArgs args)
		{
			if (combo.Active != -1)
				actionGroups.RemoveAt (combo.Active);
		}
		
		void OnActiveChanged (object s, EventArgs args)
		{
			if (!updating) {
				UpdateActionCommands (null);
				if (combo.Active != -1)
					currentGroup = (ActionGroup) actionGroups [combo.Active];
				else
					currentGroup = null;
				if (ActiveGroupChanged != null)
					ActiveGroupChanged (this, new ActionGroupEventArgs (ActiveGroup));
			}
		}
		
		void UpdateActionCommands (Action action)
		{
			foreach (Gtk.Widget w in Children) {
				if (!internalButtons.Contains (w))
					Remove (w);
			}
			AddActionCommands (action);
				
			if (internalButtons.Count > 0 && internalButtons.Count != Children.Length) {
				Insert (new Gtk.SeparatorToolItem (), internalButtons.Count);
			}
			ShowAll ();
		}
		
		protected virtual void AddActionCommands (Action action)
		{
		}
		
		public event ActionGroupEventHandler ActiveGroupChanged;
		public event ActionGroupEventHandler ActiveGroupCreated;
	}
}

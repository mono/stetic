
using System;
using System.Collections;
using Gtk;

namespace Stetic
{
	
	public class WidgetActionBar: Gtk.Toolbar
	{
		Project project;
		Stetic.Wrapper.Widget rootWidget;
		WidgetTreeCombo combo;
		ToolItem comboItem;
		Stetic.Wrapper.Widget selection;
		Hashtable editors;
		Hashtable sensitives, invisibles;
		
		public WidgetActionBar (Stetic.Wrapper.Widget rootWidget)
		{
			editors = new Hashtable ();
			sensitives = new Hashtable ();
			invisibles = new Hashtable ();

			IconSize = IconSize.SmallToolbar;
			Orientation = Orientation.Horizontal;
			ToolbarStyle = ToolbarStyle.BothHoriz;

			combo = new WidgetTreeCombo ();
			comboItem = new ToolItem ();
			comboItem.Add (combo);
			comboItem.ShowAll ();
			Insert (comboItem, -1);
			ShowAll ();
			RootWidget = rootWidget;
		}
		
		public override void Dispose ()
		{
			Clear ();
			base.Dispose ();
		}
		
		public Stetic.Wrapper.Widget RootWidget {
			get { return rootWidget; }
			set {
				if (project != null) {
					project.SelectionChanged -= new Wrapper.WidgetEventHandler (OnSelectionChanged);
					project = null;
				}
				
				rootWidget = value;
				combo.RootWidget = rootWidget;
				
				if (rootWidget != null) {
					project = (Stetic.Project) rootWidget.Project;
					project.SelectionChanged += new Wrapper.WidgetEventHandler (OnSelectionChanged);
				}
			}
		}
		
		void Clear ()
		{
			if (selection != null)
				selection.Notify -= Notified;
			selection = null;
			
			editors.Clear ();
			sensitives.Clear ();
			invisibles.Clear ();
				
			foreach (Gtk.Widget child in Children)
				if (child != comboItem)
					Remove (child);
		}
		
		void OnSelectionChanged (object s, Wrapper.WidgetEventArgs args)
		{
			Clear ();
			selection = args.Widget;
			
			if (selection == null) {
				combo.SetSelection (null);
				return;
			}

			// Look for the root widget, and only update the bar if the selected
			// widget is a child of the root widget
			
			Stetic.Wrapper.Widget w = selection;
			while (w != null && !w.IsTopLevel) {
				w = Stetic.Wrapper.Container.LookupParent ((Gtk.Widget) w.Wrapped);
			}
			if (w == null || w != rootWidget)
				return;

			combo.SetSelection (selection);
			selection.Notify += Notified;
			AddWidgetCommands (selection);
			UpdateSensitivity ();
		}
		
		protected virtual void AddWidgetCommands (Stetic.Wrapper.Widget widget)
		{
			foreach (ItemGroup igroup in widget.ClassDescriptor.ItemGroups) {
				foreach (ItemDescriptor desc in igroup) {
					if (desc is CommandDescriptor)
						AppendCommand ((CommandDescriptor) desc);
				}
			}
		}
		
		void AppendCommand (CommandDescriptor cmd)
		{
			Gtk.ToolButton button = new Gtk.ToolButton (null, null);
			button.Label = cmd.Label;
			button.IsImportant = true;
			button.Clicked += delegate (object o, EventArgs args) {
				cmd.Run (selection.Wrapped);
			};
			button.Show ();
			Insert (button, -1);

			if (cmd.HasDependencies) {
				editors[cmd.Name] = button;
				sensitives[cmd] = cmd;
			}
			if (cmd.HasVisibility) {
				editors[cmd.Name] = button;
				invisibles[cmd] = cmd;
			}
		}
		
		void Notified (object s, string propertyName)
		{
			UpdateSensitivity ();
		}
		
		void UpdateSensitivity ()
		{
			foreach (ItemDescriptor item in sensitives.Keys) {
				Widget w = editors[item.Name] as Widget;
				if (w != null) {
					object ob = sensitives.Contains (item) ? selection.Wrapped : null;
					w.Sensitive = item.EnabledFor (ob);
				}
			}
			foreach (ItemDescriptor item in invisibles.Keys) {
				Widget w = editors[item.Name] as Widget;
				if (w != null) {
					object ob = invisibles.Contains (item) ? selection.Wrapped : null;
					w.Visible = item.VisibleFor (ob);
				}
			}
		}
	}
}

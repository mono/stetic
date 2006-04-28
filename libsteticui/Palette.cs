using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.VBox, IComparer {

		Hashtable groups;
		Project project;
		WidgetLibrary[] libraries;
		ArrayList visibleGroups = new ArrayList ();
		Group localActionGroup;
		Wrapper.Widget selection;

		class Group : Gtk.Expander {
		
			private Gtk.Alignment align;
			private Gtk.VBox vbox;
			
			public Group (string name) : base ("<b>" + name + "</b>")
			{
				vbox = new VBox (false, 5);
				
				align = new Gtk.Alignment (0, 0, 0, 0);
				align.SetPadding (0, 0, 20, 0);
				align.Child = vbox;

				UseMarkup = true;
				Expanded = true;
				Child = align;
			}
			
			public void Append (Widget w)
			{
				vbox.PackStart (w, false, false, 0);
			} 
		}

		public Palette () : base (false, 2)
		{
			groups = new Hashtable ();
			Registry.RegistryChanged += OnRegistryChanged;
			
			ShowGroup ("window", "Windows");
			ShowGroup ("widget", "Widgets");
			ShowGroup ("container", "Containers");
			ShowGroup ("toolbaritem", "Toolbar Items");
		}
		
		public override void Dispose ()
		{
			Registry.RegistryChanged -= OnRegistryChanged;
			base.Dispose ();
		}
		
		public Palette (Project project): this ()
		{
			this.Project = project;
		}
		
		public Project Project {
			get { return project; }
			set {
				if (project != null)
					project.SelectionChanged -= OnSelectionChanged;
				project = value;
				if (project != null)
					project.SelectionChanged += OnSelectionChanged;
				LoadWidgets (project);
			}
		}
		
		public WidgetLibrary[] WidgetLibraries {
			get { return libraries; }
			set { 
				libraries = value; 
				LoadWidgets (project);
			}
		}
		
		void OnSelectionChanged (object ob, Stetic.Wrapper.WidgetEventArgs args)
		{
			selection = args.Widget;
			FillLocalActionGroup (localActionGroup);
		}
		
		public void ShowGroup (string name, string label)
		{
			visibleGroups.Add (new string[] { name, label });
			if (project != null)
				LoadWidgets (project);
		}
		
		public void HideGroup (string name)
		{
			for (int n=0; n < visibleGroups.Count; n++) {
				if (((string[])visibleGroups[n])[0] == name) {
					visibleGroups.RemoveAt (n);
					if (project != null)
						LoadWidgets (project);
					return;
				}
			}
		}
		
		void OnRegistryChanged (object o, EventArgs args)
		{
			LoadWidgets (project);
		}
		
		public void LoadWidgets (Project project)
		{
			foreach (Group g in groups.Values)
				Remove (g);
				
			groups.Clear ();
			
			foreach (string[] grp in visibleGroups)
				AddOrGetGroup (grp[0], grp[1]);

			ArrayList classes = new ArrayList ();
			if (libraries == null) {
				foreach (ClassDescriptor klass in Registry.AllClasses)
					classes.Add (klass);
			} else {
				classes.AddRange (Registry.CoreWidgetLibrary.AllClasses);
				foreach (WidgetLibrary lib in libraries)
					if (lib != Registry.CoreWidgetLibrary)
						classes.AddRange (lib.AllClasses);
			}
			
			classes.Sort (this);

			foreach (ClassDescriptor klass in classes) {
				if (klass.Deprecated || klass.Category == "")
					continue;

				if (!groups.Contains (klass.Category))
					continue;
					
				WidgetFactory factory;
				if (klass.Category == "window")
					factory = new WindowFactory (project, klass);
				else
					factory = new WidgetFactory (project, klass);

				AddOrGetGroup(klass.Category).Append (factory);
			}
			
			localActionGroup = AddOrGetGroup ("action-group - local", "Actions");
			FillLocalActionGroup (localActionGroup);
			foreach (Stetic.Wrapper.ActionGroup group in project.ActionGroups) {
				ShowActionGroup (group);
			}
			ShowAll ();
		}

		int IComparer.Compare (object x, object y)
		{
			return string.Compare (((ClassDescriptor)x).Label,
					       ((ClassDescriptor)y).Label);
		}
		
		void ShowActionGroup (Stetic.Wrapper.ActionGroup group)
		{
			Group g = AddOrGetGroup ("action-group " + group.Name, group.Name);
			FillActionGroup (g, group);
		}
		
		void FillLocalActionGroup (Group widgetGroup)
		{
			Stetic.Wrapper.ActionPaletteItem it = new Stetic.Wrapper.ActionPaletteItem (Gtk.UIManagerItemType.Menuitem, null, null);
			widgetGroup.Append (new InstanceWidgetFactory ("Empty Action", null, it));
			
			it = new Stetic.Wrapper.ActionPaletteItem (Gtk.UIManagerItemType.Menu, null, null);
			widgetGroup.Append (new InstanceWidgetFactory ("Action Menu", null, it));
			
			if (selection != null && selection.LocalActionGroup != null)
				FillActionGroup (widgetGroup, selection.LocalActionGroup);
		}
		
		void FillActionGroup (Group widgetGroup, Stetic.Wrapper.ActionGroup group)
		{
			foreach (Stetic.Wrapper.Action action in group.Actions) {
				Gdk.Pixbuf icon = Gtk.IconTheme.Default.LoadIcon (action.GtkAction.StockId, 16, 0);
				Stetic.Wrapper.ActionPaletteItem it = new Stetic.Wrapper.ActionPaletteItem (Gtk.UIManagerItemType.Menuitem, null, action);
				widgetGroup.Append (new InstanceWidgetFactory (action.GtkAction.Label, icon, it));
			}
		}

		private Group AddOrGetGroup (string id, string name)
		{
			Group group = (Group) groups[id];

			if (group == null) {
				group = new Group (name);
				PackStart (group, false, false, 0);
				groups.Add (id, group);
			}

			return group;
		}

		private Group AddOrGetGroup (string name)
		{
			return AddOrGetGroup (name, name);
		}
	}
}

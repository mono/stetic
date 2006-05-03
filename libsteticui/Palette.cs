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
		Wrapper.Widget selection;
		ArrayList actionGroups = new ArrayList ();
		
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
#if ACTIONS			
			FillLocalActionGroup ();
			ShowAll ();
#endif
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
			foreach (PaletteGroup g in groups.Values)
				Remove (g);
				
			// Unsubscribe events from old action groups
			foreach (Wrapper.ActionGroup g in actionGroups) {
				g.ActionAdded -= OnActionGroupChanged;
				g.ActionRemoved -= OnActionGroupChanged;
			}
			actionGroups.Clear ();

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

#if ACTIONS			
			if (project != null) {
				FillLocalActionGroup ();
				foreach (Stetic.Wrapper.ActionGroup group in project.ActionGroups) {
					ShowActionGroup (group);
				}
			}
#endif			
			ShowAll ();
		}

		int IComparer.Compare (object x, object y)
		{
			return string.Compare (((ClassDescriptor)x).Label,
					       ((ClassDescriptor)y).Label);
		}
		
		void ShowActionGroup (Stetic.Wrapper.ActionGroup group)
		{
			PaletteGroup g = AddOrGetActionGroup ("action-group " + group.Name, group.Name, group);
			FillActionGroup (g, group);
		}
		
		void FillLocalActionGroup ()
		{
			if (selection != null && selection.LocalActionGroup != null) {
				PaletteGroup widgetGroup = AddOrGetActionGroup ("action-group - local", "Actions", selection.LocalActionGroup);
				widgetGroup.Clear ();
				FillActionGroup (widgetGroup, selection.LocalActionGroup);
			} else {
				PaletteGroup pg = (PaletteGroup) groups ["action-group - local"];
				if (pg != null)
					pg.Clear ();
			}
		}
		
		void FillActionGroup (PaletteGroup widgetGroup, Stetic.Wrapper.ActionGroup group)
		{
			if (!actionGroups.Contains (group)) {
				actionGroups.Add (group);
				group.ActionAdded += OnActionGroupChanged;
				group.ActionRemoved += OnActionGroupChanged;
			}
			
			foreach (Stetic.Wrapper.Action action in group.Actions) {
				Gdk.Pixbuf icon;
				try {
					icon = Gtk.IconTheme.Default.LoadIcon (action.GtkAction.StockId, 16, 0);
				} catch {
					icon = Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, 16, 0);
				}
				Stetic.Wrapper.ActionPaletteItem it = new Stetic.Wrapper.ActionPaletteItem (Gtk.UIManagerItemType.Menuitem, null, action);
				widgetGroup.Append (new InstanceWidgetFactory (action.GtkAction.Label, icon, it));
			}
		}
		
		void OnActionGroupChanged (object s, Stetic.Wrapper.ActionEventArgs args)
		{
			LoadWidgets (project);
		}

		private PaletteGroup AddOrGetGroup (string id, string name)
		{
			PaletteGroup group = (PaletteGroup) groups[id];

			if (group == null) {
				group = new PaletteGroup (name);
				PackStart (group, false, false, 0);
				groups.Add (id, group);
			}

			return group;
		}

		private PaletteGroup AddOrGetActionGroup (string id, string name, Wrapper.ActionGroup group)
		{
			PaletteGroup pg = (PaletteGroup) groups[id];

			if (pg == null) {
				pg = new ActionPaletteGroup (name, group);
				PackStart (pg, false, false, 0);
				groups.Add (id, pg);
			}

			return pg;
		}

		private PaletteGroup AddOrGetGroup (string name)
		{
			return AddOrGetGroup (name, name);
		}
	}

	class PaletteGroup : Gtk.Expander
	{
		private Gtk.Alignment align;
		private Gtk.VBox vbox;
		Gtk.Label emptyLabel;
		bool isEmpty = true;
		
		public PaletteGroup (string name) : base ("<b>" + name + "</b>")
		{
			vbox = new VBox (false, 5);
			emptyLabel = new Gtk.Label ();
			emptyLabel.Markup = "<small><i><span foreground='darkgrey'>  Empty</span></i></small>";
			vbox.PackStart (emptyLabel, false, false, 0);
			
			align = new Gtk.Alignment (0, 0, 0, 0);
			align.SetPadding (0, 0, 20, 0);
			align.Child = vbox;

			UseMarkup = true;
			Expanded = true;
			Child = align;
		}
		
		public void Append (Widget w)
		{
			if (isEmpty) {
				vbox.Remove (emptyLabel);
				isEmpty = false;
			}
			vbox.PackStart (w, false, false, 0);
		} 
	
		public void Clear ()
		{
			foreach (Gtk.Widget w in vbox.Children)
				vbox.Remove (w);

			isEmpty = true;
			vbox.PackStart (emptyLabel, false, false, 0);
		}
	}
	
	class ActionPaletteGroup : PaletteGroup 
	{
		Wrapper.ActionGroup group;
		
		public ActionPaletteGroup (string name, Wrapper.ActionGroup group): base (name)
		{
			DND.DestSet (this, true);
			this.group = group;
		}
		
		protected override bool OnDragDrop (Gdk.DragContext context, int x,	int y, uint time)
		{
			Wrapper.ActionPaletteItem dropped = DND.Drop (context, null, time) as Wrapper.ActionPaletteItem;
			if (dropped == null)
				return false;

			if (dropped.Node.Action.ActionGroup != group) {
				dropped.Node.Action.ActionGroup.Actions.Remove (dropped.Node.Action);
				group.Actions.Add (dropped.Node.Action);
			}

			return base.OnDragDrop (context, x,	y, time);
		}
	}

}

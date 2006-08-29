using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;
using Mono.Unix;

namespace Stetic {

	public class Palette : Gtk.VBox, IComparer {

		Hashtable groups;
		Project project;
		WidgetLibrary[] libraries;
		ArrayList visibleGroups = new ArrayList ();
		Wrapper.Widget selection;
		ActionGroupBox localActionsBox;
		ActionGroupBox globalActionsBox;
		
		public Palette () : base (false, 0)
		{
			groups = new Hashtable ();
			Registry.RegistryChanged += OnRegistryChanged;
			
			ShowGroup ("window", Catalog.GetString ("Windows"));
			ShowGroup ("widget", Catalog.GetString ("Widgets"));
			ShowGroup ("container", Catalog.GetString ("Containers"));
//			ShowGroup ("toolbaritem", "Toolbar Items");
			ShowGroup ("actions", Catalog.GetString ("Actions"));
		}
		
		public override void Dispose ()
		{
			Registry.RegistryChanged -= OnRegistryChanged;
			
			foreach (PaletteGroup grp in groups.Values)
				grp.Destroy ();

			if (localActionsBox != null) {
				localActionsBox.Destroy ();
				localActionsBox = null;
			}
			if (globalActionsBox != null) {
				globalActionsBox.Destroy ();
				globalActionsBox = null;
			}
			
			project = null;
			selection = null;
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
			selection = args.WidgetWrapper;
			localActionsBox.SetActionGroups (selection != null ? selection.LocalActionGroups : null);
			ShowAll ();
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
			foreach (PaletteGroup g in groups.Values) {
				Remove (g);
				g.Destroy ();
			}
				
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

			if (localActionsBox != null)
				localActionsBox.Destroy ();
			if (globalActionsBox != null)
				globalActionsBox.Destroy ();
				
			PaletteGroup widgetGroup = AddOrGetGroup ("actions", Catalog.GetString ("Actions"));
			localActionsBox = new ActionGroupBox ();
			globalActionsBox = new ActionGroupBox ();
			widgetGroup.Append (localActionsBox);
			widgetGroup.Append (globalActionsBox);
			
			if (project != null) {
				widgetGroup.Sensitive = true;
				localActionsBox.SetActionGroups (selection != null ? selection.LocalActionGroups : null);
				globalActionsBox.SetActionGroups (project.ActionGroups);
			} else {
				widgetGroup.Sensitive = false;
				localActionsBox.SetActionGroups (null);
				globalActionsBox.SetActionGroups (null);
			}
			ShowAll ();
		}

		int IComparer.Compare (object x, object y)
		{
			return string.Compare (((ClassDescriptor)x).Label,
					       ((ClassDescriptor)y).Label);
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
			vbox = new VBox (false, 0);
			emptyLabel = new Gtk.Label ();
			emptyLabel.Markup = "<small><i><span foreground='darkgrey'>  " + Catalog.GetString ("Empty") + "</span></i></small>";
			vbox.PackStart (emptyLabel, false, false, 0);
			
			align = new Gtk.Alignment (0, 0, 0, 0);
			align.SetPadding (0, 0, 20, 0);
			align.Child = vbox;

			UseMarkup = true;
			Expanded = true;
			Child = align;
		}
		
		public void SetName (string name)
		{
			Label = "<b>" + name + "</b>";
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
			foreach (Gtk.Widget w in vbox.Children) {
				vbox.Remove (w);
				w.Destroy ();
			}

			isEmpty = true;
			vbox.PackStart (emptyLabel, false, false, 0);
		}
	}
	
	class ActionPaletteGroup : PaletteGroup 
	{
		Wrapper.ActionGroup group;
		static Gdk.Pixbuf defaultActionIcon;
		
		static ActionPaletteGroup ()
		{
			defaultActionIcon = Gdk.Pixbuf.LoadFromResource ("action.png");
		}
		
		public ActionPaletteGroup (string name, Wrapper.ActionGroup group): base (name)
		{
			DND.DestSet (this, true);
			this.group = group;
			group.ActionAdded += OnActionGroupChanged;
			group.ActionRemoved += OnActionGroupChanged;
			group.ActionChanged += OnActionGroupChanged;
			group.Changed += OnActionGroupChanged;
			Fill ();
		}
		
		public Wrapper.ActionGroup Group {
			get { return group; }
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			group.ActionAdded -= OnActionGroupChanged;
			group.ActionRemoved -= OnActionGroupChanged;
			group.ActionChanged -= OnActionGroupChanged;
			group.Changed -= OnActionGroupChanged;
		}
		
		public void Fill ()
		{
			foreach (Stetic.Wrapper.Action action in group.Actions) {
				Gdk.Pixbuf icon = action.RenderIcon (Gtk.IconSize.Menu);
				if (icon == null) icon = defaultActionIcon;
				Stetic.Wrapper.ActionPaletteItem it = new Stetic.Wrapper.ActionPaletteItem (Gtk.UIManagerItemType.Menuitem, null, action);
				Append (new InstanceWidgetFactory (action.MenuLabel, icon, it));
			}
		}
		
		void OnActionGroupChanged (object s, EventArgs args)
		{
			SetName (((Stetic.Wrapper.ActionGroup)s).Name);
		}
		
		void OnActionGroupChanged (object s, Stetic.Wrapper.ActionEventArgs args)
		{
			Clear ();
			Fill ();
			ShowAll ();
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

	class ActionGroupBox: Gtk.VBox
	{
		Stetic.Wrapper.ActionGroupCollection groups;
		
		public void SetActionGroups (Stetic.Wrapper.ActionGroupCollection groups)
		{
			if (this.groups != null) {
				this.groups.ActionGroupAdded -= OnGroupAdded;
				this.groups.ActionGroupRemoved -= OnGroupRemoved;
			}
			this.groups = groups;
			if (this.groups != null) {
				this.groups.ActionGroupAdded += OnGroupAdded;
				this.groups.ActionGroupRemoved += OnGroupRemoved;
			}
			Update ();
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			foreach (ActionPaletteGroup grp in Children)
				grp.Destroy ();
			SetActionGroups (null);
		}
		
		public void Update ()
		{
			foreach (ActionPaletteGroup grp in Children) {
				Remove (grp);
				grp.Destroy ();
			}
			
			if (groups != null) {
				foreach (Stetic.Wrapper.ActionGroup group in groups) {
					ActionPaletteGroup pg = new ActionPaletteGroup (group.Name, group);
					PackStart (pg, false, false, 0);
				}
			}
			ShowAll ();
		}
		
		void OnGroupAdded (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			ActionPaletteGroup pg = new ActionPaletteGroup (args.ActionGroup.Name, args.ActionGroup);
			pg.ShowAll ();
			PackStart (pg, false, false, 0);
		}
		
		void OnGroupRemoved (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			foreach (ActionPaletteGroup grp in Children) {
				if (grp.Group == args.ActionGroup) {
					Remove (grp);
					grp.Destroy ();
				}
			}
		}
	}
}

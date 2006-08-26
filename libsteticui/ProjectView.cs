using Gtk;
using System;
using Mono.Unix;

namespace Stetic {

	public class ProjectView : NodeView 
	{
		Project project;
		
		public event Wrapper.WidgetEventHandler WidgetActivated;
		
		public ProjectView ()
		{
			TreeViewColumn col;
			CellRenderer renderer;

			col = new TreeViewColumn ();
			col.Title = Catalog.GetString ("Widget");

			renderer = new CellRendererPixbuf ();
			col.PackStart (renderer, false);
			col.AddAttribute (renderer, "pixbuf", 0);

			renderer = new CellRendererText ();
			col.PackStart (renderer, true);
			col.AddAttribute (renderer, "text", 1);

			AppendColumn (col);

			NodeSelection.Mode = SelectionMode.Single;
			NodeSelection.Changed += RowSelected;
		}
		
		public ProjectView (Project project): this ()
		{
			this.Project = project;
		}
		
		public Project Project {
			get { return project; }
			set {
				if (project != null) {
					project.SelectionChanged -= WidgetSelected;
					project.ProjectReloaded -= OnProjectReloaded;
					project.WidgetNameChanged -= OnWidgetNameChanged;
				}
				project = value;
				if (project != null) {
					NodeStore = project.Store;
					project.SelectionChanged += WidgetSelected;
					project.ProjectReloaded += OnProjectReloaded;
					project.WidgetNameChanged += OnWidgetNameChanged;
				} else {
					NodeStore = null;
				}
			}
		}
		
		void OnProjectReloaded (object o, EventArgs a)
		{
			NodeStore = project.Store;
		}
		
		Stetic.Wrapper.Widget SelectedWrapper {
			get {
				ITreeNode[] nodes = NodeSelection.SelectedNodes;

				if (nodes == null || nodes.Length == 0)
					return null;

				ProjectNode node = (ProjectNode)nodes[0];
				return Stetic.Wrapper.Widget.Lookup (node.Widget);
			}
		}

		bool syncing = false;

		void RowSelected (object obj, EventArgs args)
		{
			if (!syncing) {
				syncing = true;
				Stetic.Wrapper.Widget selection = SelectedWrapper;
				if (selection != null) {
					selection.Select ();
				}
				syncing = false;
			}
		}

		void WidgetSelected (object s, Wrapper.WidgetEventArgs args)
		{
			if (!syncing) {
				syncing = true;
				if (args.Widget != null) {
					ProjectNode node = project.GetNode (args.Widget);
					if (node != null)
						NodeSelection.SelectNode (node);
				}
				else
					NodeSelection.UnselectAll ();
				syncing = false;
			}
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			if (evt.Button == 3 && evt.Type == Gdk.EventType.ButtonPress)
				return OnPopupMenu ();
			return base.OnButtonPressEvent (evt);
		}

		protected override bool OnPopupMenu ()
		{
			Stetic.Wrapper.Widget selection = SelectedWrapper;

			if (selection != null) {
				Menu m = new ContextMenu (selection);
				if (selection.IsTopLevel) {
					// Allow deleting top levels from the project view
					ImageMenuItem item = new ImageMenuItem (Gtk.Stock.Delete, null);
					item.Activated += delegate (object obj, EventArgs args) {
						selection.Delete ();
					};
					item.Show ();
					m.Add (item);
				}
				m.Popup ();
				return true;
			} else
				return base.OnPopupMenu ();
		}

		void OnWidgetNameChanged (object s, Wrapper.WidgetNameChangedArgs args)
		{
			QueueDraw ();
		}
		
		protected override void OnRowActivated (TreePath path, TreeViewColumn col)
		{
			base.OnRowActivated (path, col);
			Stetic.Wrapper.Widget w = SelectedWrapper;
			if (w != null && WidgetActivated != null)
				WidgetActivated (this, new Wrapper.WidgetEventArgs (w));
		}
	}
}

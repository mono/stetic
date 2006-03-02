using Gtk;
using System;

namespace Stetic {

	public class ProjectView : NodeView 
	{
		Project project;
		
		public ProjectView ()
		{
			TreeViewColumn col;
			CellRenderer renderer;

			col = new TreeViewColumn ();
			col.Title = "Widget";

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
					project.Selected -= WidgetSelected;
					project.ProjectReloaded -= OnProjectReloaded;
					project.WidgetNameChanged -= OnWidgetNameChanged;
				}
				project = value;
				if (project != null) {
					NodeStore = project.Store;
					project.Selected += WidgetSelected;
					project.ProjectReloaded += OnProjectReloaded;
					project.WidgetNameChanged += OnWidgetNameChanged;
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

		void WidgetSelected (Gtk.Widget selection, ProjectNode node)
		{
			if (!syncing) {
				syncing = true;
				if (node != null)
					NodeSelection.SelectNode (node);
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
				m.Popup ();
				return true;
			} else
				return base.OnPopupMenu ();
		}

		void OnWidgetNameChanged (object s, Wrapper.WidgetNameChangedArgs args)
		{
			QueueDraw ();
		}
	}
}

using Gtk;
using System;

namespace Stetic {

	public class ProjectView : NodeView {
		public ProjectView (Project project) : base (project.Store)
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
			NodeSelection.Changed += Selection_Changed;
		}

		IWidgetSite SelectedSite {
			get {
				ITreeNode[] nodes = NodeSelection.SelectedNodes;

				if (nodes == null || nodes.Length == 0)
					return null;

				ProjectNode node = (ProjectNode)nodes[0];

				Widget w = node.Widget.Parent;
				while (w != null && !(w is IWidgetSite))
					w = w.Parent;

				if (w == null)
					return WindowSite.LookupSite (node.Widget);
				else
					return (IWidgetSite)w;
			}
		}

		void Selection_Changed (object obj, EventArgs args)
		{
			IWidgetSite selection = SelectedSite;

			if (selection != null)
				selection.Select ();
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			if (evt.Button == 3 && evt.Type == Gdk.EventType.ButtonPress)
				return OnPopupMenu ();
			return base.OnButtonPressEvent (evt);
		}

		protected override bool OnPopupMenu ()
		{
			IWidgetSite selection = SelectedSite;

			if (selection != null) {
				Menu m = new ContextMenu (selection);
				m.Popup ();
				return true;
			} else
				return base.OnPopupMenu ();
		}


	}
}

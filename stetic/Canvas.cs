using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public class Canvas : Gtk.Layout {

		Project project;

		public Canvas (Project project) : base (null, null)
		{
			this.project = project;
			DND.DestSet (this, false);
		}

		protected override void OnRealized ()
		{
			base.OnRealized ();
			ModifyBg (Gtk.StateType.Normal, Style.Base (Gtk.StateType.Normal));
		}

		protected override bool OnDragMotion (Gdk.DragContext context,
						      int x, int y, uint time)
		{
			Gdk.Drag.Status (context, Gdk.DragAction.Move, time);
			return true;
		}

		protected override bool OnDragDrop (Gdk.DragContext context,
						    int x, int y, uint time)
		{
			Stetic.Wrapper.Widget wrapper = DND.Drop (context, this, time);
			if (wrapper == null)
				return false;

			Gtk.Widget dropped = wrapper.Wrapped;
			if (dropped is Gtk.Window)
				dropped = EmbedWindow.Wrap ((Gtk.Window)dropped);
			Put (dropped, x, y);

			wrapper.Select ();
			return true;
		}

		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y,
							    Gtk.SelectionData selectionData,
							    uint info, uint time)
		{
			Stetic.Wrapper.Widget dropped =
				GladeUtils.Paste (project, selectionData);

			Gtk.Drag.Finish (context, dropped != null, dropped != null, time);
			if (dropped != null)
				Put (dropped.Wrapped, x, y);
		}
	}
}

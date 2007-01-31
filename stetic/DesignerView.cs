
using Gtk;
using System;
using Mono.Unix;

namespace Stetic
{
	public class DesignerView: Gtk.Notebook
	{
		WidgetDesigner design;
		Gtk.Widget actionbox;
		Component widget;
		
		public DesignerView (Stetic.Project project, Component widget)
		{
			this.widget = widget;

			// Widget design tab
			
			design = project.CreateWidgetDesigner (widget, true);
			
			// Actions design tab
			
			actionbox = design.CreateActionGroupDesigner ();
			
			// Designers tab
			
			AppendPage (design, new Gtk.Label (Catalog.GetString ("Designer")));
			AppendPage (actionbox, new Gtk.Label (Catalog.GetString ("Actions")));
			TabPos = Gtk.PositionType.Bottom;
		}
		
		public Component Component {
			get { return widget; }
		}
		
		public UndoQueue UndoQueue {
			get { return design.UndoQueue; }
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			design.Dispose ();
			actionbox.Dispose ();
		}
	}
}
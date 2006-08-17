
using Gtk;
using System;
using Mono.Unix;

namespace Stetic
{
	public class DesignerView: Gtk.Notebook
	{
		Gtk.Widget design;
		Gtk.Widget actionbox;
		
		public DesignerView (Stetic.Project project, Gtk.Container widget)
		{
			Stetic.Wrapper.Container wc = Stetic.Wrapper.Container.Lookup (widget);
			
			// Widget design tab
			
			design = UserInterface.CreateWidgetDesigner (widget);
			VBox box = new VBox ();
			box.BorderWidth = 3;
			box.PackStart (new WidgetActionBar (wc), false, false, 0);
			box.PackStart (design, true, true, 3);
			
			// Actions design tab
			
			actionbox = UserInterface.CreateActionGroupDesigner (project, wc.LocalActionGroups);
			
			// Designers tab
			
			AppendPage (box, new Gtk.Label (Catalog.GetString ("Designer")));
			AppendPage (actionbox, new Gtk.Label (Catalog.GetString ("Actions")));
			TabPos = Gtk.PositionType.Bottom;
		}
		
		public override void Dispose ()
		{
			design.Dispose ();
			actionbox.Dispose ();
		}
	}
}

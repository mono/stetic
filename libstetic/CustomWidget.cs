
using System;

namespace Stetic
{
	// This widget is used at design-time to represent a Gtk.Bin container.
	// Gtk.Bin is the base class for custom widgets.
	
	public class CustomWidget: Gtk.Bin
	{
		Gtk.Widget child;
		
		protected override void OnSizeRequested (ref Gtk.Requisition req)
		{
			if (child != null)
				req = child.SizeRequest ();
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if (child != null)
				child.Allocation = allocation;
		}
		
		protected override void OnAdded (Gtk.Widget widget)
		{
			base.OnAdded (widget);
			child = widget;
		}
	}
	
}

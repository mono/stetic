using System;

namespace Stetic 
{
	public interface IProject 
	{
		string FileName { get; }
		
		Gtk.Widget Selection { get; set; }
		
		IResourceProvider ResourceProvider { get; set; }

		bool Modified { get; set; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);

		void AddWindow (Gtk.Window window);
		
		Wrapper.ActionGroupCollection ActionGroups { get; }
		ProjectIconFactory IconFactory { get; }
		
		event Wrapper.WidgetEventHandler SelectionChanged;
	}
}

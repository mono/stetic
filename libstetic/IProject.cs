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

		string ImagesRootPath { get; }
		string TargetGtkVersion { get; }
		
		string ImportFile (string filePath);
		
		event Wrapper.WidgetEventHandler SelectionChanged;
	}
}

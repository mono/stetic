using System;

namespace Stetic 
{
	public interface IProject 
	{
		string FileName { get; }
		Gtk.Widget[] Toplevels { get; }
		Gtk.Widget Selection { get; set; }
		Wrapper.ActionGroupCollection ActionGroups { get; }
		ProjectIconFactory IconFactory { get; }
		string ImagesRootPath { get; }
		string TargetGtkVersion { get; }
		bool Modified { get; set; }
		IResourceProvider ResourceProvider { get; set; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);
		void AddWindow (Gtk.Window window);
		string ImportFile (string filePath);
		
		event Wrapper.WidgetEventHandler SelectionChanged;
	}
}

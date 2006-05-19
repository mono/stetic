using System;

namespace Stetic {
	public delegate void IProjectDelegate ();

	public interface IProject {
	
		string FileName { get; }
		
		Gtk.Widget Selection { get; set; }
		
		IResourceProvider ResourceProvider { get; set; }

		Stetic.Tooltips Tooltips { get; }
		
		bool Modified { get; set; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);

		Gtk.Widget LookupWidgetById (string id);

		void AddWindow (Gtk.Window window);
		
		Wrapper.ActionGroupCollection ActionGroups { get; }
		ProjectIconFactory IconFactory { get; }
		
		event IProjectDelegate GladeImportComplete;
		event Wrapper.WidgetEventHandler SelectionChanged;
	}
}

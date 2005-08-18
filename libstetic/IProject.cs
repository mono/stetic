using System;

namespace Stetic {
	public delegate void IProjectDelegate ();

	public interface IProject {
		Gtk.Widget Selection { get; set; }

		Stetic.Tooltips Tooltips { get; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);

		Gtk.Widget LookupWidgetById (string id);

		event IProjectDelegate GladeImportComplete;

		void AddWindow (Gtk.Window window);
	}
}

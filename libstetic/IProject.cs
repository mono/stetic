using System;

namespace Stetic {
	public delegate void IProjectDelegate ();

	public interface IProject {
		Gtk.Widget Selection { get; set; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);

		Gtk.Widget LookupWidgetById (string id);

		event IProjectDelegate GladeImportComplete;
	}
}

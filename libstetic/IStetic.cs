using System;

namespace Stetic {
	public delegate void ISteticDelegate ();

	public interface IStetic {
		Stetic.Wrapper.Widget Selection { get; set; }

		void PopupContextMenu (Stetic.Wrapper.Widget wrapper);
		void PopupContextMenu (Placeholder ph);

		Gtk.Widget LookupWidgetById (string id);

		event ISteticDelegate GladeImportComplete;
	}
}

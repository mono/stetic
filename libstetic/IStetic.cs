using System;

namespace Stetic {
	public delegate void GladeImportCompleteDelegate ();

	public interface IStetic {
		WidgetSite CreateWidgetSite ();

		Gtk.Widget LookupWidgetById (string id);

		event GladeImportCompleteDelegate GladeImportComplete;
	}
}

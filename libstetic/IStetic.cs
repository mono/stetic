using System;

namespace Stetic {
	public delegate void ISteticDelegate ();

	public interface IStetic {
		WidgetSite CreateWidgetSite (Gtk.Widget w);
		Placeholder CreatePlaceholder ();

		Gtk.Widget LookupWidgetById (string id);

		event ISteticDelegate GladeImportComplete;
	}
}

using System;

namespace Stetic {
	public delegate void ISteticDelegate ();

	public interface IStetic {
		WidgetSite CreateWidgetSite (Gtk.Widget w);
		Placeholder CreatePlaceholder ();

		void Select (Stetic.Wrapper.Widget wrapper);

		Gtk.Widget LookupWidgetById (string id);

		event ISteticDelegate GladeImportComplete;
	}
}

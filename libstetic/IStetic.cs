using System;

namespace Stetic {
	public delegate void ISteticDelegate ();

	public interface IStetic {
		WidgetSite CreateWidgetSite ();

		Gtk.Widget LookupWidgetById (string id);

		event ISteticDelegate GladeImportComplete;

		event ISteticDelegate DragBegin;
		event ISteticDelegate DragEnd;

		void DragBegun ();
		void DragEnded ();
	}
}

using Gtk;
using System;

namespace Stetic {

	public interface IWidgetSite {
		Widget Contents { get; }
		IWidgetSite ParentSite { get; }

		bool Occupied { get; }

		void Select ();
		void UnSelect ();

		void Delete ();
	}

}

using System;

namespace Stetic {
	public interface IStetic {
		WidgetSite CreateWidgetSite ();
		WidgetSite CreateWidgetSite (int emptyWidth, int emptyHeight);
	}
}

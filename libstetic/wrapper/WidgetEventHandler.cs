using System;

namespace Stetic.Wrapper
{
	public delegate void WidgetEventHandler (object sender, WidgetEventArgs args);
	
	public class WidgetEventArgs: EventArgs
	{
		Stetic.Wrapper.Widget widget;
		
		public WidgetEventArgs (Stetic.Wrapper.Widget widget)
		{
			this.widget = widget;
		}
		
		public Stetic.Wrapper.Widget Widget {
			get { return widget; }
		}
	}	
}

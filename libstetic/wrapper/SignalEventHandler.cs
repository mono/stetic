
namespace Stetic.Wrapper
{
	public delegate void SignalEventHandler (object sender, SignalEventArgs args);
	
	public class SignalEventArgs: WidgetEventArgs
	{
		public Signal signal;
		
		public SignalEventArgs (Widget widget, Signal signal): base (widget)
		{
			this.signal = signal;
		}
		
		public Signal Signal {
			get { return signal; }
		}
	}
}

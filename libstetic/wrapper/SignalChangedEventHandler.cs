
namespace Stetic.Wrapper
{
	public delegate void SignalChangedEventHandler (object sender, SignalChangedEventArgs args);
	
	public class SignalChangedEventArgs: SignalEventArgs
	{
		public Signal oldSignal;
		
		public SignalChangedEventArgs (Wrapper.Widget widget, Signal oldSignal, Signal signal): base (widget, signal)
		{
			this.oldSignal = oldSignal;
		}
		
		public Signal OldSignal {
			get { return oldSignal; }
		}
	}
}


using System;

namespace Stetic
{
	public class SignalsEditor: PluggableWidget
	{
		SignalsEditorFrontend frontend;
		SignalsEditorEditSession session;
		
		public event EventHandler SignalActivated;
		
		internal SignalsEditor (Application app): base (app)
		{
			frontend = new SignalsEditorFrontend (this);
		}
		
		public Signal SelectedSignal {
			get {
				if (session != null)
					return session.SelectedSignal;
				else
					return null; 
			}
		}
		
		protected override void OnCreatePlug (uint socketId)
		{
			session = app.Backend.CreateSignalsWidgetPlug (frontend, socketId);
		}
		
		protected override Gtk.Widget OnCreateWidget ()
		{
			session = app.Backend.GetSignalsWidget (frontend);
			return session.Editor;
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			session.Dispose ();
			System.Runtime.Remoting.RemotingServices.Disconnect (frontend);
		}
		
		internal void NotifySignalActivated ()
		{
			if (SignalActivated != null)
				SignalActivated (this, EventArgs.Empty);
		}
	}
	
	internal class SignalsEditorFrontend: MarshalByRefObject
	{
		SignalsEditor editor;
		
		public SignalsEditorFrontend (SignalsEditor editor)
		{
			this.editor = editor;
		}
		
		public void NotifySignalActivated ()
		{
			Gtk.Application.Invoke (
				delegate {
					editor.NotifySignalActivated ();
				}
			);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
	}
}

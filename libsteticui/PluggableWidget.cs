
using System;

namespace Stetic
{
	public abstract class PluggableWidget: Gtk.EventBox
	{
		internal Application app;
		bool initialized;
		Gtk.Socket socket;
		bool customWidget;
		
		public PluggableWidget (Application app)
		{
			this.app = app;
			if (app.UseExternalBackend) {
				app.BackendChanged += OnBackendChanged;
				app.BackendChanging += OnBackendChanging;
			}
		}
		
		protected void AddCustomWidget (Gtk.Widget w)
		{
			if (initialized) {
				Gtk.Widget cw = Child;
				Remove (Child);
				cw.Destroy ();
			}
			else
				initialized = true;
			Add (w);
			w.ShowAll ();
			customWidget = true;
		}
		
		protected void ResetCustomWidget ()
		{
			customWidget = false;
		}
		
		protected override void OnRealized ()
		{
			base.OnRealized ();
			if (!initialized && !app.Disposed) {
				initialized = true;
				if (app.UseExternalBackend)
					ConnectPlug ();
				else {
					Gtk.Widget w = OnCreateWidget ();
					w.Show ();
					Add (w);
				}
			}
		}
		
		protected override void OnUnrealized ()
		{
			if (!app.Disposed && app.UseExternalBackend && initialized) {
				OnDestroyPlug (socket.Id);
				initialized = false;
			}
			base.OnUnrealized ();
		}
		
		protected void UpdateWidget ()
		{
			if (!initialized || app.Disposed)
				return;

			if (!app.UseExternalBackend) {
				if (Child != null) {
					Gtk.Widget cw = Child;
					Remove (Child);
					cw.Destroy ();
				}
				Gtk.Widget w = OnCreateWidget ();
				w.Show ();
				Add (w);
			}
		}
		
		protected abstract void OnCreatePlug (uint socketId);
		protected abstract void OnDestroyPlug (uint socketId);
		
		protected abstract Gtk.Widget OnCreateWidget ();
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (app.UseExternalBackend) {
				app.BackendChanged -= OnBackendChanged;
				app.BackendChanging -= OnBackendChanging;
			}
		}
		
		internal virtual void OnBackendChanged (ApplicationBackend oldBackend)
		{
			if (!initialized || app.Disposed)
				return;

			if (app.UseExternalBackend) {
				Gtk.Widget w = Child;
				Remove (Child);
				w.Destroy ();
				socket.Dispose ();
				ConnectPlug ();
			}
		}
		
		internal virtual void OnBackendChanging ()
		{
		}
		
		void ConnectPlug ()
		{
			socket = new Gtk.Socket ();
			socket.Show ();
			Add (socket);
			OnCreatePlug (socket.Id);
		}
	}
}

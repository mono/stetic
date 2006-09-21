
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
			if (initialized)
				Remove (Child);
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
			if (!initialized) {
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
		
		protected void UpdateWidget ()
		{
			if (initialized) {
				if (Child != null)
					Remove (Child);
				Gtk.Widget w = OnCreateWidget ();
				w.Show ();
				Add (w);
			}
		}
		
		protected abstract void OnCreatePlug (uint socketId);
		
		protected abstract Gtk.Widget OnCreateWidget ();
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (app.UseExternalBackend) {
				app.BackendChanged -= OnBackendChanged;
				app.BackendChanging -= OnBackendChanging;
			}
		}
		
		void OnBackendChanging (object ob, EventArgs args)
		{
			OnBackendChanging ();
		}
		
		void OnBackendChanged (object ob, EventArgs args)
		{
			OnBackendChanged ();
		}
		
		protected virtual void OnBackendChanged ()
		{
			if (initialized && app.UseExternalBackend && !customWidget) {
				Remove (Child);
				socket.Dispose ();
				ConnectPlug ();
			}
		}
		
		protected virtual void OnBackendChanging ()
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

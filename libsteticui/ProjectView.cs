
using System;

namespace Stetic
{
	public class ProjectView: PluggableWidget
	{
		ProjectViewFrontend frontend;
		
		internal ProjectView (Application app): base (app)
		{
			frontend = new ProjectViewFrontend (app);
		}
		
		public event ComponentEventHandler ComponentActivated {
			add { frontend.ComponentActivated += value; }
			remove { frontend.ComponentActivated -= value; }
		}
		
		protected override void OnCreatePlug (uint socketId)
		{
			app.Backend.CreateProjectWidgetPlug (frontend, socketId);
		}
		
		protected override void OnDestroyPlug (uint socketId)
		{
			app.Backend.DestroyProjectWidgetPlug ();
		}
		
		protected override Gtk.Widget OnCreateWidget ()
		{
			return app.Backend.GetProjectWidget (frontend);
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			System.Runtime.Remoting.RemotingServices.Disconnect (frontend);
		}
	}
	
	
	internal class ProjectViewFrontend: MarshalByRefObject
	{
		Application app;
		
		public event ComponentEventHandler ComponentActivated;
		
		public ProjectViewFrontend (Application app)
		{
			this.app = app;
		}
		
		public void NotifyWidgetActivated (object ob, string widgetName, string widgetType)
		{
			Gtk.Application.Invoke (
				delegate {
					Component c = app.GetComponent (ob, widgetName, widgetType);
					if (c != null && ComponentActivated != null)
						ComponentActivated (null, new ComponentEventArgs (app.ActiveProject, c));
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

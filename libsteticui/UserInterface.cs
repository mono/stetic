
using System;

namespace Stetic
{
	public class UserInterface
	{
		UserInterface()
		{
		}
		
		public static WidgetDesigner CreateWidgetDesigner (Gtk.Container widget)
		{
			Stetic.Wrapper.Container wc = Stetic.Wrapper.Container.Lookup (widget);
			return CreateWidgetDesigner (widget, wc.DesignWidth, wc.DesignHeight);
		}
		
		public static WidgetDesigner CreateWidgetDesigner (Gtk.Container widget, int designWidth, int designHeight)
		{
			return EmbedWindow.Wrap (widget, designWidth, designHeight);
		}
		
		public static Gtk.Widget CreateActionGroupDesigner (Project project, Wrapper.ActionGroupCollection actionGroups)
		{
			Editor.ActionGroupEditor agroupEditor = new Editor.ActionGroupEditor ();
			agroupEditor.Project = project;
			Gtk.Widget groupDesign = EmbedWindow.Wrap (agroupEditor, -1, -1);
			
			Gtk.VBox actionbox = new Gtk.VBox ();
			actionbox.BorderWidth = 3;
			ActionGroupToolbar groupToolbar = new ActionGroupToolbar (actionGroups);
			groupToolbar.Bind (agroupEditor);
			
			actionbox.PackStart (groupToolbar, false, false, 0);
			actionbox.PackStart (groupDesign, true, true, 3);

			return actionbox;
		}
		
		public static IObjectViewer DefaultPropertyViewer {
			get { return WidgetDesigner.DefaultPropertyViewer; }
			set { WidgetDesigner.DefaultPropertyViewer = value; }
		}
		
		public static IObjectViewer DefaultSignalsViewer {
			get { return WidgetDesigner.DefaultSignalsViewer; }
			set { WidgetDesigner.DefaultSignalsViewer = value; }
		}
	}
}

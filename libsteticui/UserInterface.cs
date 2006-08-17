
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
		
		public static ActionGroupDesigner CreateActionGroupDesigner (Project project, Wrapper.ActionGroupCollection actionGroups)
		{
			ActionGroupToolbar groupToolbar = new ActionGroupToolbar (actionGroups);
			return CreateActionGroupDesigner (project, groupToolbar);
		}
		
		public static ActionGroupDesigner CreateActionGroupDesigner (Project project, ActionGroupToolbar groupToolbar)
		{
			Editor.ActionGroupEditor agroupEditor = new Editor.ActionGroupEditor ();
			agroupEditor.Project = project;
			WidgetDesigner groupDesign = EmbedWindow.Wrap (agroupEditor, -1, -1);
			
			groupToolbar.Bind (agroupEditor);
			
			return new ActionGroupDesigner (groupDesign, agroupEditor, groupToolbar);
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

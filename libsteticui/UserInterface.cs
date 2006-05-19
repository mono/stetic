
using System;

namespace Stetic
{
	public class UserInterface
	{
		UserInterface()
		{
		}
		
		public static PreviewBox CreateWidgetDesigner (Gtk.Container widget, int designWidth, int designHeight)
		{
			return EmbedWindow.Wrap (widget, designWidth, designHeight);
		}
		
		public static PreviewBox CreateActionGroupDesigner (Project project, Wrapper.ActionGroupCollection actionGroups)
		{
			Editor.ActionGroupEditor agroupEditor = new Editor.ActionGroupEditor ();
			agroupEditor.Project = project;
			return EmbedWindow.Wrap (agroupEditor, -1, -1);
		}
		
		public static IObjectViewer DefaultPropertyViewer {
			get { return PreviewBox.DefaultPropertyViewer; }
			set { PreviewBox.DefaultPropertyViewer = value; }
		}
		
		public static IObjectViewer DefaultSignalsViewer {
			get { return PreviewBox.DefaultSignalsViewer; }
			set { PreviewBox.DefaultSignalsViewer = value; }
		}
	}
}

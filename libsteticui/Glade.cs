using System;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

namespace Stetic {

	public static class GladeFiles {

		public static void Import (Project project, string filename)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (filename);
			project.SetFileName (filename);
			project.Id = System.IO.Path.GetFileName (filename);
			doc = GladeUtils.XslImportTransform (doc);

			XmlNode node = doc.SelectSingleNode ("/glade-interface");
			if (node == null)
				throw new ApplicationException ("Not a glade file according to node name");

			try {
				project.BeginGladeImport ();
				foreach (XmlElement toplevel in node.SelectNodes ("widget")) {
					Wrapper.Container wrapper = Stetic.ObjectWrapper.Read (project, toplevel, FileFormat.Glade) as Wrapper.Container;
					if (wrapper != null)
						project.AddWidget ((Gtk.Widget)wrapper.Wrapped);
				}
			} finally {
				project.EndGladeImport ();
			}
		}

		public static void Export (Project project, string filename)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			XmlElement toplevel = doc.CreateElement ("glade-interface");
			doc.AppendChild (toplevel);

			foreach (Widget w in project.Toplevels) {
				Stetic.Wrapper.Container wrapper = Stetic.Wrapper.Container.Lookup (w);
				if (wrapper == null)
					continue;

				XmlElement elem = wrapper.Write (doc, FileFormat.Glade);
				if (elem != null)
					toplevel.AppendChild (elem);
			}
	
			doc = GladeUtils.XslExportTransform (doc);

			// FIXME; if you use UTF8, it starts with a BOM???
			XmlTextWriter writer = new XmlTextWriter (filename, System.Text.Encoding.ASCII);
			writer.Formatting = Formatting.Indented;
			doc.Save (writer);
			writer.Close ();
		}
	}
}
using System;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

namespace Stetic {

	public static class GladeFiles {

		public const string Glade20SystemId = "http://glade.gnome.org/glade-2.0.dtd";

		public static void Import (Project project, string filename)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (filename);

			XmlDocumentType doctype = doc.DocumentType;
			if (doctype == null ||
			    doctype.Name != "glade-interface" ||
			    doctype.SystemId != Glade20SystemId)
				throw new ApplicationException ("Not a glade file according to doctype");

			XmlReader reader = Registry.GladeImportXsl.Transform (doc, null, (XmlResolver)null);
			doc = new XmlDocument ();
			doc.Load (reader);

			XmlNode node = doc.SelectSingleNode ("/glade-interface");
			if (node == null)
				throw new ApplicationException ("Not a glade file according to node name");

			project.BeginGladeImport ();
			foreach (XmlNode toplevel in node.SelectNodes ("widget")) {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (project, (XmlElement)toplevel);
				if (wrapper != null)
					project.AddWindow ((Gtk.Window)wrapper.Wrapped);
			}
			project.EndGladeImport ();
		}

		public static void Export (Project project, string filename)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			XmlDocumentType doctype = doc.CreateDocumentType ("glade-interface", null, Glade20SystemId, null);
			doc.AppendChild (doctype);

			XmlElement toplevel = doc.CreateElement ("glade-interface");
			doc.AppendChild (toplevel);

			foreach (Widget w in project.Toplevels) {
				Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (w);
				if (wrapper == null)
					continue;

				XmlElement elem = wrapper.GladeExport (doc);
				if (elem != null)
					toplevel.AppendChild (elem);
			}

			// FIXME; if you use UTF8, it starts with a BOM???
			XmlTextWriter writer = new XmlTextWriter (filename, System.Text.Encoding.ASCII);
			writer.Formatting = Formatting.Indented;
			Registry.GladeExportXsl.Transform (doc, null, writer, null);
			writer.Close ();
		}
	}
}

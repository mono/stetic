using System;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

namespace Stetic {

	public static class Glade {

		public const string Glade20SystemId = "http://glade.gnome.org/glade-2.0.dtd";

		public static void Import (string filename, Project project)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			XmlTextReader reader = null;
			try {
				reader = new XmlTextReader (filename);
				doc.Load (reader);
			} catch {
				return;
			} finally {
				if (reader != null)
					reader.Close ();
			}

			XmlDocumentType doctype = doc.DocumentType;
			if (doctype == null ||
			    doctype.Name != "glade-interface" ||
			    doctype.SystemId != Glade20SystemId)
				throw new ApplicationException ("Not a glade file according to doctype");

			XmlNode node;

			node = doc.SelectSingleNode ("/glade-interface");
			if (node == null)
				throw new ApplicationException ("Not a glade file according to node name");

			project.BeginGladeImport ();
			foreach (XmlNode toplevel in node.SelectNodes ("widget")) {
				Widget w = CreateWidget (project, null, null, toplevel);
				if (w == null)
					continue;
				project.AddWindow (w);
				WindowSite site = new WindowSite ((Gtk.Window)w);
				site.FocusChanged += delegate (WindowSite site, IWidgetSite focus) {
					if (focus == null)
						SteticMain.NoSelection ();
					else
						SteticMain.Select (focus);
				};
			}
			project.EndGladeImport ();
		}

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_child_property (ref GLib.Value value, string className, string propertyName);

		static Widget CreateWidget (Project project, Stetic.Wrapper.Container parent, XmlNode thischild, XmlNode widget)
		{
			string className = widget.Attributes["class"].Value;
			string id = widget.Attributes["id"].Value;

			Hashtable props;
			ExtractProperties (widget.SelectNodes ("property"), out props);

			ObjectWrapper wrapper;
			try {
				if (thischild == null) {
					wrapper = Stetic.ObjectWrapper.GladeImport (project, className, id, props);
				} else if (thischild.Attributes["internal-child"] != null) {
					wrapper = parent.GladeSetInternalChild (thischild.Attributes["internal-child"].Value,
										className, id, props);
				} else {
					Hashtable childprops;
					ExtractProperties (thischild.SelectNodes ("packing/property"), out childprops);
					wrapper = parent.GladeImportChild (className, id, props, childprops);
				}
			} catch (GladeException ge) {
				Console.Error.WriteLine ("Could not import widget: {0}", ge);
				return null;
			}

			Stetic.Wrapper.Container container = wrapper as Stetic.Wrapper.Container;
			if (container != null) {
				foreach (XmlNode child in widget.SelectNodes ("child")) {
					widget = child.SelectSingleNode ("widget");

					if (widget == null)
						container.AddPlaceholder ();
					else
						CreateWidget (project, container, child, widget);
				}
			}

			return (Gtk.Widget)wrapper.Wrapped;
		}

		static void ExtractProperties (XmlNodeList nodes, out Hashtable props)
		{
			props = new Hashtable ();
			foreach (XmlNode prop in nodes)
				props[prop.Attributes["name"].Value] = prop.InnerText;
		}
	}
}

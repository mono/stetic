using System;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

namespace Stetic {

	public static class Glade {

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

			XmlNode node;

			node = doc.SelectSingleNode ("/glade-interface");
			if (node == null)
				throw new ApplicationException ("Not a glade file according to node name");

			project.BeginGladeImport ();
			foreach (XmlNode toplevel in node.SelectNodes ("widget")) {
				Stetic.Wrapper.Window window =
					ImportWidget (project, null, null, toplevel) as Stetic.Wrapper.Window;
				if (window != null)
					project.AddWindow (window);
			}
			project.EndGladeImport ();
		}

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_child_property (ref GLib.Value value, string className, string propertyName);

		static Stetic.Wrapper.Widget ImportWidget (Project project, Stetic.Wrapper.Container parent, XmlNode thischild, XmlNode widget)
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
				Console.Error.WriteLine ("Could not import widget: {0}", ge.Message);
				return null;
			}

			Stetic.Wrapper.Container container = wrapper as Stetic.Wrapper.Container;
			if (container != null) {
				foreach (XmlNode child in widget.SelectNodes ("child")) {
					widget = child.SelectSingleNode ("widget");

					if (widget == null)
						container.AddPlaceholder ();
					else
						ImportWidget (project, container, child, widget);
				}
			}

			return (Stetic.Wrapper.Widget)wrapper;
		}

		static void ExtractProperties (XmlNodeList nodes, out Hashtable props)
		{
			props = new Hashtable ();
			foreach (XmlNode prop in nodes)
				props[prop.Attributes["name"].Value] = prop.InnerText;
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
				XmlElement element = ExportWidget (project, doc, null, w);
				if (element != null)
					toplevel.AppendChild (element);
			}

			// FIXME; if you use UTF8, it starts with a BOM???
			XmlTextWriter writer = new XmlTextWriter (filename, System.Text.Encoding.ASCII);
			writer.Formatting = Formatting.Indented;
			doc.Save (writer);
		}

		static XmlElement ExportWidget (Project project, XmlDocument doc,
						Stetic.Wrapper.Container parent, Widget w)
		{
			XmlElement ret;

			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (w);
			if (wrapper == null)
				return null;

			string className, id, internalId;
			Hashtable props, childprops;
			try {
				if (parent != null)
					parent.GladeExportChild (wrapper, out className, out internalId, out id, out props, out childprops);
				else {
					wrapper.GladeExport (out className, out id, out props);
					internalId = null;
					childprops = null;
				}
			} catch (GladeException ge) {
				Console.Error.WriteLine ("Could not export widget: {0}", ge.Message);
				return null;
			}

			XmlElement widget = doc.CreateElement ("widget");
			widget.SetAttribute ("class", className);
			widget.SetAttribute ("id", id);
			SetProps (widget, props);

			if (parent == null)
				ret = widget;
			else {
				XmlElement child, packing;

				child = doc.CreateElement ("child");
				if (internalId != null)
					child.SetAttribute ("internal-child", internalId);
				child.AppendChild (widget);

				if (childprops != null && childprops.Count > 0) {
					packing = doc.CreateElement ("packing");
					SetProps (packing, childprops);
					child.AppendChild (packing);
				}
				ret = child;
			}

			Stetic.Wrapper.Container container = wrapper as Stetic.Wrapper.Container;
			if (container != null) {
				XmlElement elt;

				foreach (Gtk.Widget child in container.RealChildren) {
#if FIXME
					if (!site.Occupied) {
						child = doc.CreateElement ("child");
						child.AppendChild (doc.CreateElement ("placeholder"));
						widget.AppendChild (child);
						continue;
					}
#endif

					elt = ExportWidget (project, doc, container, child);
					if (elt != null)
						widget.AppendChild (elt);
				}
			}

			return ret;
		}

		static void SetProps (XmlElement element, Hashtable props)
		{
			foreach (string name in props.Keys) {
				string val = props[name] as string;

				XmlElement property = element.OwnerDocument.CreateElement ("property");
				property.SetAttribute ("name", name);
				if (val != "")
					property.InnerText = val;
				element.AppendChild (property);
			}
		}
	}
}

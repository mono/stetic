using System;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

using Gtk;

namespace Stetic {

	public class GladeImport {

		static WidgetFactory empty_istetic;

		public const string Glade20SystemId = "http://glade.gnome.org/glade-2.0.dtd";

		static MethodInfo setProperty = typeof (GLib.Object).GetMethod ("SetProperty", BindingFlags.NonPublic | BindingFlags.Instance);
		
		public static void Load (string filename, Project project)
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

			ArrayList propNames, propVals;
			ExtractProperties (widget.SelectNodes ("property"), out propNames, out propVals);

			ObjectWrapper wrapper;
			if (thischild == null) {
				wrapper = Stetic.ObjectWrapper.GladeImport (project, className, id, propNames, propVals);
			} else if (thischild.Attributes["internal-child"] != null) {
				wrapper = parent.GladeSetInternalChild (thischild.Attributes["internal-child"].Value,
									className, id,
									propNames, propVals);
			} else {
				ArrayList packingNames, packingVals;
				ExtractProperties (thischild.SelectNodes ("packing/property"),
						   out packingNames, out packingVals);
				wrapper = parent.GladeImportChild (className, id,
								   propNames, propVals,
								   packingNames, packingVals);
			}

			if (wrapper == null) {
				Console.WriteLine ("Could not create stetic wrapper for type {0}", className);
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

		static void ExtractProperties (XmlNodeList nodes, out ArrayList names, out ArrayList values)
		{
			names = new ArrayList ();
			values = new ArrayList ();
			foreach (XmlNode prop in nodes) {
				names.Add (prop.Attributes["name"].Value);
				values.Add (prop.InnerText);
			}
		}
	}
}

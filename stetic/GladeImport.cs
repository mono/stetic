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

			node = doc.SelectSingleNode ("//glade-interface");
			if (node == null || node.Name != "glade-interface")
				throw new ApplicationException ("Not a glade file according to node name");

			for (node = node.FirstChild; node != null; node = node.NextSibling) {
				if (node.Name == "widget") {
					Widget w = CreateWidget (node);
					project.AddWindow (w);
					WindowSite site = new WindowSite ((Gtk.Window)w);
					site.FocusChanged += delegate (WindowSite site, IWidgetSite focus) {
						if (focus == null)
							SteticMain.NoSelection ();
						else
							SteticMain.Select (focus);
					};
				} else
					Console.WriteLine ("Skipping {0} node", node.Name);
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_property (ref GLib.Value value, string className, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_child_property (ref GLib.Value value, string className, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_hydrate (ref GLib.Value value, string data);

		static Widget CreateWidget (XmlNode wnode)
		{
			string className = wnode.Attributes["class"].InnerText;

			string[] propNamesArray;
			GLib.Value[] propValuesArray;

			HydrateProperties (className, wnode.SelectNodes ("property"), out propNamesArray, out propValuesArray);
			
			IntPtr raw = gtksharp_object_newv (g_type_from_name (className), propNamesArray.Length, propNamesArray, propValuesArray);
			
			if (raw == IntPtr.Zero) {
				return null;
			}

			Widget widget = (Widget)GLib.Object.GetObject (raw, true);
			if (widget == null) {
				return null;
			}

			widget.Name = wnode.Attributes["id"].InnerText;

			if (!(widget is Gtk.Container)) {
				CreateWrapperType (widget);
				return widget;
			}
			Gtk.Container container = (Gtk.Container)widget;

			Stetic.Wrapper.Container wrapper = (Stetic.Wrapper.Container) CreateWrapperType (widget);
			AddChildren (container, wrapper, className, wnode.SelectNodes ("child"));

			return widget;
		}

		static void AddChildren (Gtk.Container container, Stetic.Wrapper.Container wrapper, string className, XmlNodeList children)
		{
			foreach (XmlNode node in children) {

				Widget childw = null;
			
				bool addWithSite = true;
				if (node.Attributes["internal-child"] != null) {
					object internalChildWidget = FindInternalChild (container, node.Attributes["internal-child"].InnerText);
					
					if (internalChildWidget == null)
						continue;
					
					string[] internalPropNames;
					GLib.Value[] internalValues;
					HydrateProperties (node.SelectSingleNode("widget").Attributes["class"].InnerText, node.SelectNodes ("widget/property"), out internalPropNames, out internalValues);
					childw = (Widget)internalChildWidget;
					for (int i = 0; i < internalPropNames.Length; i++)
					{
						setProperty.Invoke (childw, new object[] { internalPropNames[i], internalValues[i] } );
					}
					
					if (childw is Gtk.Container) { 
						AddChildren ((Gtk.Container)childw, (Stetic.Wrapper.Container)CreateWrapperType (childw), node.SelectSingleNode("widget").Attributes["class"].InnerText, node.SelectNodes ("widget/child"));
					}

					addWithSite = false;
				} else {
					XmlNode child = node.SelectSingleNode ("widget");
					if (child != null)
						childw = CreateWidget (child);
					else
						wrapper.AddSite ();
				}
				
				if (childw == null)
					continue;
			
				ArrayList childPropNames = new ArrayList ();
				ArrayList childPropValues = new ArrayList ();
				
				foreach (XmlNode prop in node.SelectNodes ("packing/property")) {
					if (container is Gtk.Frame && prop.Attributes["name"] != null && prop.Attributes["name"].InnerText == "type" && prop.InnerText == "label_item") {
						addWithSite = false;
						((Gtk.Frame)container).LabelWidget = childw;
						break;
					}
					string propName = prop.Attributes["name"].InnerText;
					GLib.Value value = new GLib.Value ();
					if (!stetic_g_value_init_for_child_property (ref value, className, propName))
						continue;
					if (!stetic_g_value_hydrate (ref value, prop.InnerText))
						continue;

					childPropNames.Add (propName);
					childPropValues.Add (value);

				}

				if (addWithSite) {
					WidgetSite site = wrapper.AddSite (childw);
					childw = site;
				}
				
				for (int i = 0; i < childPropNames.Count; i++)
				{
					container.ChildSetProperty (childw, (string)childPropNames[i], (GLib.Value)childPropValues[i]);
				}
			}
		}

		static void HydrateProperties (string className, XmlNodeList nodes, out string[] propNamesArray, out GLib.Value[] propValuesArray)
		{
			ArrayList propNames = new ArrayList ();
			ArrayList values = new ArrayList ();
			foreach (XmlNode node in nodes) {

				// FIXME: These need to be masked because they
				// cause a window to be shown right away
				string propName = node.Attributes["name"].InnerText;
				if (propName == "visible" && (className == "GtkWindow" || className == "GtkDialog"))
					continue;

				string stringValue = node.InnerText;

				// FIXME: invisible_char is a guint, but glade
				// serializes it as a string, so this makes
				// it the guint that gtk expects.
				if (propName == "invisible_char")
					stringValue = ((int)stringValue[0]).ToString ();

				GLib.Value value = new GLib.Value ();

				if (!stetic_g_value_init_for_property (ref value, className, propName))
					continue;
				if (!stetic_g_value_hydrate (ref value, stringValue))
					continue;
				
				propNames.Add (propName);
				values.Add (value);
			}
			propNamesArray = (string[])propNames.ToArray (typeof (string));
			propValuesArray = (GLib.Value[])values.ToArray (typeof (GLib.Value));
		}

		static object FindInternalChild (Gtk.Widget container, string propertyName)
		{
			Type type = container.GetType ();
			PropertyInfo pinfo = type.GetProperty (propertyName.Replace ("_", ""), BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
			if (pinfo != null)
				return pinfo.GetValue (container, null);
			
			if (container.Parent != null)
				return FindInternalChild (container.Parent, propertyName);
			return null;
		}

		static object CreateWrapperType (Widget widget)
		{
			Type[] wrapper_types = ObjectWrapper.LookupWrapperTypes (widget);
			if (wrapper_types == null) {
				return null;
			}

			Type final_wrapper_type = null;
			if (wrapper_types.Length == 1) {
				final_wrapper_type = wrapper_types[0];
			} else {
				if (((Gtk.Image)widget).Stock != String.Empty) {
					final_wrapper_type = typeof (Stetic.Wrapper.Icon);
				} else {
					final_wrapper_type = typeof (Stetic.Wrapper.Image);
				}
			}

			// FIXME: This is needed because the only thing that
			// implements the IStetic interface is a WidgetFactory.
			if (empty_istetic == null) {
				empty_istetic = new WidgetFactory ("null", Gdk.Pixbuf.LoadFromResource ("missing.png"), typeof (Stetic.Wrapper.Label));
			}
			return Activator.CreateInstance (final_wrapper_type, new object[] { empty_istetic, widget, true } );
		}
	}
}

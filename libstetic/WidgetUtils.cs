using System;
using System.Reflection;
using System.Collections;
using System.Xml;
using Stetic.Wrapper;

namespace Stetic
{
	public static class WidgetUtils
	{
		static Gdk.Atom steticAtom;
		
		public static Gdk.Atom ApplicationXSteticAtom {
			get {
				if (steticAtom == null)
					steticAtom = Gdk.Atom.Intern ("application/x-stetic", false);
				return steticAtom;
			}
		}

		public static XmlElement ExportWidget (Gtk.Widget widget)
		{
			XmlDocument doc = new XmlDocument ();
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			if (wrapper == null)
				throw new InvalidOperationException ();
				
			XmlElement elem = wrapper.Write (doc, FileFormat.Native);
			doc.AppendChild (elem);
			return doc.DocumentElement;
		}
		
		public static Gtk.Widget ImportWidget (IProject project, XmlElement element)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.Read (project, element, FileFormat.Native);
			return wrapper.Wrapped as Gtk.Widget;
		}
		
		public static XmlElement Write (ObjectWrapper wrapper, XmlDocument doc)
		{
			ClassDescriptor klass = wrapper.ClassDescriptor;

			XmlElement elem = doc.CreateElement ("widget");
			elem.SetAttribute ("class", klass.Name);
			elem.SetAttribute ("id", ((Gtk.Widget)wrapper.Wrapped).Name);

			GetProps (wrapper, elem);
			GetSignals (wrapper, elem);
			return elem;
		}

		public static void GetProps (ObjectWrapper wrapper, XmlElement parent_elem)
		{
			ClassDescriptor klass = wrapper.ClassDescriptor;

			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null)
						continue;
					if (!prop.VisibleFor (wrapper.Wrapped) || !prop.CanWrite || prop.Name == "Name")	// Name is written in the id attribute
						continue;

					object value = prop.GetValue (wrapper.Wrapped);
					
					// If the property has its default value, we don't need to write it
					if (value == null || (prop.HasDefault && prop.IsDefaultValue (value)))
						continue;
				
					string val = prop.ValueToString (value);
					if (val == null)
						continue;

					XmlElement prop_elem = parent_elem.OwnerDocument.CreateElement ("property");
					prop_elem.SetAttribute ("name", prop.Name);
					if (val.Length > 0)
						prop_elem.InnerText = val;

					if (prop.Translatable && prop.IsTranslated (wrapper.Wrapped)) {
						prop_elem.SetAttribute ("translatable", "yes");
						if (prop.TranslationContext (wrapper.Wrapped) != null) {
							prop_elem.SetAttribute ("context", "yes");
							prop_elem.InnerText = prop.TranslationContext (wrapper.Wrapped) + "|" + prop_elem.InnerText;
						}
						if (prop.TranslationComment (wrapper.Wrapped) != null)
							prop_elem.SetAttribute ("comments", prop.TranslationComment (wrapper.Wrapped));
					}

					parent_elem.AppendChild (prop_elem);
				}
			}
		}

		static void GetSignals (ObjectWrapper wrapper, XmlElement parent_elem)
		{
			Stetic.Wrapper.Widget ob = wrapper as Stetic.Wrapper.Widget;
			if (ob == null) return;
			
			foreach (Signal signal in ob.Signals) {
				if (!signal.SignalDescriptor.VisibleFor (wrapper.Wrapped))
					continue;

				XmlElement signal_elem = parent_elem.OwnerDocument.CreateElement ("signal");
				signal_elem.SetAttribute ("name", signal.SignalDescriptor.Name);
				signal_elem.SetAttribute ("handler", signal.Handler);
				if (signal.After)
					signal_elem.SetAttribute ("after", "yes");
				parent_elem.AppendChild (signal_elem);
			}
		}
		
		static public void Read (ObjectWrapper wrapper, XmlElement elem)
		{
			string className = elem.GetAttribute ("class");
			if (className == null)
				throw new GladeException ("<widget> node with no class name");

			ClassDescriptor klass = Registry.LookupClassByName (className);
			if (klass == null)
				throw new GladeException ("No stetic ClassDescriptor for " + className);

			Gtk.Widget widget = (Gtk.Widget) wrapper.Wrapped;
			if (widget == null) {
				widget = (Gtk.Widget) klass.CreateInstance (wrapper.Project);
				widget.Name = elem.GetAttribute ("id");
				ObjectWrapper.Bind (wrapper.Project, klass, wrapper, widget, true);
			}
			else
				widget.Name = elem.GetAttribute ("id");
			
			ReadSignals (klass, wrapper, elem);
			ReadProperties (klass, wrapper, widget, elem);
			
			if (!(widget is Gtk.Window))
				widget.ShowAll ();
		}

		static void ReadSignals (ClassDescriptor klass, ObjectWrapper wrapper, XmlElement elem)
		{
			Stetic.Wrapper.Widget ob = wrapper as Stetic.Wrapper.Widget;
			if (ob == null) return;
			
			foreach (ItemGroup group in klass.SignalGroups) {
				foreach (SignalDescriptor signal in group) {
					XmlElement signal_elem = elem.SelectSingleNode ("signal[@name='" + signal.Name + "']") as XmlElement;
					if (signal_elem == null)
						continue;
					
					string handler = signal_elem.GetAttribute ("handler");
					bool after = signal_elem.GetAttribute ("after") == "yes";
					ob.Signals.Add (new Signal (signal, handler, after));
				}
			}
		}
		
		static void ReadProperties (ClassDescriptor klass, ObjectWrapper wrapper, object wrapped, XmlElement elem)
		{
			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || !prop.CanWrite)
						continue;

					XmlElement prop_node = (XmlElement) elem.SelectSingleNode ("property[@name='" + prop.Name + "']");
					if (prop_node == null)
						continue;
					
					string strval = prop_node.InnerText;
					
					// Skip translation context
					if (prop_node.GetAttribute ("context") == "yes" && strval.IndexOf ('|') != -1)
						strval = strval.Substring (strval.IndexOf ('|') + 1);
						
					object value = prop.StringToValue (strval);
					prop.SetValue (wrapped, value);
					
					if (prop.Translatable) {
						if (prop_node.GetAttribute ("translatable") != "yes") {
							prop.SetTranslated (wrapped, false);
						}
						else {
							prop.SetTranslated (wrapped, true);
							if (prop_node.GetAttribute ("context") == "yes") {
								strval = prop_node.InnerText;
								int bar = strval.IndexOf ('|');
								if (bar != -1)
									prop.SetTranslationContext (wrapped, strval.Substring (0, bar));
							}

							if (prop_node.HasAttribute ("comments"))
								prop.SetTranslationComment (wrapped, prop_node.GetAttribute ("comments"));
						}
					}
				}
			}
		}
		
		static public void SetPacking (Stetic.Wrapper.Container.ContainerChild wrapper, XmlElement child_elem)
		{
			XmlElement packing = child_elem["packing"];
			if (packing == null)
				return;

			Gtk.Container.ContainerChild cc = wrapper.Wrapped as Gtk.Container.ContainerChild;
			ClassDescriptor klass = wrapper.ClassDescriptor;
			ReadProperties (klass, wrapper, cc, packing);
		}
		
		
		public static void Copy (Gtk.Widget widget, Gtk.SelectionData seldata, bool copyAsText)
		{
			XmlElement elem = ExportWidget (widget);
			if (elem == null)
				return;

			if (copyAsText)
				seldata.Text = elem.OuterXml;
			else
				seldata.Set (ApplicationXSteticAtom, 8, System.Text.Encoding.UTF8.GetBytes (elem.OuterXml));
		}

		public static Stetic.Wrapper.Widget Paste (IProject project, Gtk.SelectionData seldata)
		{
			if (seldata.Type.Name != ApplicationXSteticAtom.Name)
				return null;
				
			string data = System.Text.Encoding.UTF8.GetString (seldata.Data);
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			try {
				doc.LoadXml (data);
			} catch {
				return null;
			}
			
			Gtk.Widget w = ImportWidget (project, doc.DocumentElement);
			return Wrapper.Widget.Lookup (w);
		}
		
		public static Gtk.Widget BuildWidget (XmlDocument doc)
		{
			return BuildWidget (doc.DocumentElement);
		}

		public static Gtk.Widget BuildWidget (XmlElement elem)
		{
			string className = elem.GetAttribute ("class");
			ClassDescriptor klass = Registry.LookupClassByName (className);
			if (klass == null) return null;
			
			Gtk.Widget widget = (Gtk.Widget) klass.CreateInstance (null);
			widget.Name = elem.GetAttribute ("id");
			
			ReadProperties (klass, widget, elem);
			
			if (widget is Gtk.Container)
				BuildChildren (klass, widget as Gtk.Container, elem);

			return widget;
		}
		
		static void ReadProperties (ClassDescriptor klass, object widget, XmlElement elem)
		{
			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || !prop.CanWrite || !prop.IsRuntimeProperty)
						continue;

					XmlElement prop_node = (XmlElement) elem.SelectSingleNode ("property[@name='" + prop.Name + "']");
					if (prop_node == null)
						continue;
					
					string strval = prop_node.InnerText;
					
					object value = prop.StringToValue (strval);
					prop.SetRuntimeValue (widget, value);
				}
			}
		}
		
		static void BuildChildren (ClassDescriptor klass, Gtk.Container widget, XmlElement elem)
		{
			foreach (XmlElement child_elem in elem.SelectNodes ("./child")) {
				if (child_elem.HasAttribute ("internal-child"))
					BuildInternalChild (klass, widget, child_elem);
				else if (child_elem["widget"] != null)
					BuildChild (widget, child_elem);
			}
		}

		static void BuildChild (Gtk.Container container, XmlElement child_elem)
		{
			Gtk.Widget child = BuildWidget (child_elem["widget"]);
			container.Add (child);

			XmlElement packing = child_elem["packing"];
			if (packing == null)
				return;

			while (child != null && child.Parent != container)
				child = child.Parent;

			if (child == null)
				return;

			Gtk.Container.ContainerChild cc = container[child];
			ClassDescriptor childClass = Registry.LookupClassByName (cc.GetType().FullName);
			if (childClass != null)
				ReadProperties (childClass, cc, packing);
		}

		static void BuildInternalChild (ClassDescriptor klass, Gtk.Container container, XmlElement child_elem)
		{
			string childId = child_elem.GetAttribute ("internal-child");
			
			foreach (PropertyDescriptor prop in klass.InternalChildren) {
				if (prop.Name != childId)
					continue;
				
				Gtk.Widget child = prop.GetRuntimeValue (container) as Gtk.Widget;
				XmlElement widgetElem = child_elem["widget"];
				
				string className = widgetElem.GetAttribute ("class");
				ClassDescriptor childClass = Registry.LookupClassByName (className);
				ReadProperties (childClass, child, widgetElem);
				return;
			}
		}
	}
}

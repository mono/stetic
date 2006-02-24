using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Expander : Container {

		public static new Gtk.Expander CreateInstance ()
		{
			return new Gtk.Expander ("");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				expander.Label = expander.Name;
				AddPlaceholder ();
			}
			if (expander.LabelWidget != null)
				ObjectWrapper.Create (proj, expander.LabelWidget);
		}

		protected override Widget ReadChild (XmlElement child_elem, FileFormat format)
		{
			if ((string)GladeUtils.GetChildProperty (child_elem, "type", "") == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.Read (proj, child_elem["widget"], format);
				expander.LabelWidget = (Gtk.Widget)wrapper.Wrapped;
				return (Widget)wrapper;
			} else
				return base.ReadChild (child_elem, format);
		}

		protected override XmlElement WriteChild (Widget wrapper, XmlDocument doc, FileFormat format)
		{
			XmlElement child_elem = base.WriteChild (wrapper, doc, format);
			if (wrapper.Wrapped == expander.LabelWidget)
				GladeUtils.SetChildProperty (child_elem, "type", "label_item");
			return child_elem;
		}

		Gtk.Expander expander {
			get {
				return (Gtk.Expander)Wrapped;
			}
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			if (oldChild == expander.LabelWidget)
				expander.LabelWidget = newChild;
			else
				base.ReplaceChild (oldChild, newChild);
		}
	}
}

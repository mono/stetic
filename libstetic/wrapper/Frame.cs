using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Frame : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				frame.Label = "<b>" + frame.Name + "</b>";
				((Gtk.Label)frame.LabelWidget).UseMarkup = true;
				frame.Shadow = Gtk.ShadowType.None;
				Gtk.Alignment align = new Gtk.Alignment (0, 0, 1, 1);
				align.LeftPadding = 12;
				Container align_wrapper = (Container)ObjectWrapper.Create (proj, align);
				align_wrapper.AddPlaceholder ();
				ReplaceChild (frame.Child, (Gtk.Widget)align_wrapper.Wrapped);
			}
			ObjectWrapper.Create (proj, frame.LabelWidget);
		}

		Gtk.Frame frame {
			get {
				return (Gtk.Frame)Wrapped;
			}
		}

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			if ((string)GladeUtils.GetChildProperty (child_elem, "type", "") == "label_item") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (proj, child_elem["widget"]);
				frame.LabelWidget = (Gtk.Widget)wrapper.Wrapped;
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (child_elem);
		}

		public override XmlElement GladeExportChild (Widget wrapper, XmlDocument doc)
		{
			XmlElement child_elem = base.GladeExportChild (wrapper, doc);
			if (wrapper.Wrapped == frame.LabelWidget)
				GladeUtils.SetChildProperty (child_elem, "type", "label_item");
			return child_elem;
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			if (oldChild == frame.LabelWidget)
				frame.LabelWidget = newChild;
			else
				base.ReplaceChild (oldChild, newChild);
		}
	}
}

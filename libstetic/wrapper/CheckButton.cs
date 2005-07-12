using System;
using System.Xml;

namespace Stetic.Wrapper {

	public class CheckButton : Container {

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			hasLabel = false;
			if (checkbutton.Child != null)
				checkbutton.Remove (checkbutton.Child);
			return base.GladeImportChild (child_elem);
		}

		public Gtk.CheckButton checkbutton {
			get {
				return (Gtk.CheckButton)Wrapped;
			}
		}

		bool hasLabel = true;
		public bool HasLabel {
			get {
				return hasLabel;
			}
			set {
				hasLabel = value;
				EmitNotify ("HasLabel");
			}
		}

		internal void RemoveLabel ()
		{
			if (checkbutton.Child != null)
				checkbutton.Remove (checkbutton.Child);

			AddPlaceholder ();
			HasLabel = false;
		}

		internal void RestoreLabel ()
		{
			checkbutton.Label = checkbutton.Name;
			HasLabel = true;
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);
			EmitNotify ("HasContents");
		}

	}
}

using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class CheckButton : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.CheckButton checkbutton = (Gtk.CheckButton)obj;
				checkbutton.Label = checkbutton.Name;
			}
		}

		public override Widget GladeImportChild (string className, string id,
							 Hashtable props, Hashtable childprops)
		{
			hasLabel = false;
			return base.GladeImportChild (className, id, props, childprops);
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

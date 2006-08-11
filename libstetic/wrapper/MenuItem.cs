using System;
using System.Xml;

namespace Stetic.Wrapper {

	public class MenuItem : Container {

		public static new Gtk.MenuItem CreateInstance ()
		{
			// Use the ctor that will create an AccelLabel
			return new Gtk.MenuItem ("");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
		}

		protected override void ReadChild (XmlElement child_elem, FileFormat format)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.Read (proj, child_elem["widget"], format);
			menuitem.Submenu = (Gtk.Menu)wrapper.Wrapped;
		}

		Gtk.MenuItem menuitem {
			get {
				return (Gtk.MenuItem)Wrapped;
			}
		}

		Gtk.Label label {
			get {
				Gtk.AccelLabel label = menuitem.Child as Gtk.AccelLabel;
				if (label != null)
					return label;

				if (menuitem.Child != null)
					menuitem.Child.Destroy ();

				label = new Gtk.AccelLabel ("");
				label.MnemonicWidget = menuitem;
				label.AccelWidget = menuitem;
				label.Xalign = 0.0f;
				label.Show ();
				menuitem.Add (label);

				return label;
			}
		}

		public bool HasSubmenu {
			get {
				return menuitem.Submenu != null;
			}
		}

		string labelText;

		public string Label {
			get {
				return labelText;
			}
			set {
				label.LabelProp = labelText = value;
				EmitNotify ("Label");
			}
		}

		public bool UseUnderline {
			get {
				return label.UseUnderline;
			}
			set {
				label.UseUnderline = value;
				EmitNotify ("UseUnderline");
			}
		}

		Gtk.AccelGroup accelGroup;
		string accelerator;
		public string Accelerator {
			get {
				return accelerator;
			}
			set {
				uint key;
				Gdk.ModifierType mods;

				if (accelGroup != null && accelerator != null) {
					Gtk.Accelerator.Parse (accelerator, out key, out mods);
					menuitem.RemoveAccelerator (accelGroup, key, mods);
				}

				accelerator = value;
					
				if (accelerator != null) {
					if (accelGroup == null)
						accelGroup = new Gtk.AccelGroup ();	

					Gtk.Accelerator.Parse (accelerator, out key, out mods);
					menuitem.AddAccelerator ("activate", accelGroup, key, mods,
								 Gtk.AccelFlags.Visible);
				}

				EmitNotify ("Accelerator");
			}
		}
	}
}

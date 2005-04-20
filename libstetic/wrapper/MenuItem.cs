using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Menu Item", "menuitem.png", ObjectWrapperType.Internal)]
	public class MenuItem : Container {

		public static new Type WrappedType = typeof (Gtk.MenuItem);

		internal static new void Register (Type type)
		{
			if (type == typeof (Stetic.Wrapper.MenuItem)) {
				AddItemGroup (type,
					      "Menu Item Properties",
					      "Label",
					      "UseUnderline",
					      "Accelerator");
			}

			foreach (ItemGroup props in GetItemGroups (type)) {
				ItemDescriptor accelerator = props["Accelerator"];
				if (accelerator != null) {
					PropertyDescriptor hasSubmenu =
						new PropertyDescriptor (typeof (Stetic.Wrapper.MenuItem),
									typeof (Gtk.MenuItem),
									"HasSubmenu");
					accelerator.InvisibleIf (hasSubmenu);
				}
			}
		}

		public static new Gtk.MenuItem CreateInstance ()
		{
			// Use the ctor that will create an AccelLabel
			return new Gtk.MenuItem ("");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
		}

		public override Widget GladeImportChild (string className, string id,
							 Hashtable props, Hashtable childprops)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
			menuitem.Submenu = (Gtk.Menu)wrapper.Wrapped;
			return (Widget)wrapper;
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

		[GladeProperty (Name = "label")]
		[Description ("Label", "The text of the menu item")]
		public string Label {
			get {
				return label.LabelProp;
			}
			set {
				label.LabelProp = value;
				EmitNotify ("Label");
			}
		}

		[GladeProperty (Name = "use_underline", Proxy = "GladeUseUnderline")]
		[Description ("Use Underline", "If set, an underline in the text indicates the next character should be used for the mnemonic accelerator key")]
		public bool UseUnderline {
			get {
				return label.UseUnderline;
			}
			set {
				label.UseUnderline = value;
				EmitNotify ("UseUnderline");
			}
		}

		internal string GladeUseUnderline {
			get {
				return UseUnderline ? "True" : "False";
			}
			set {
				UseUnderline = (value == "True");
			}
		}

		Gtk.AccelGroup accelGroup;
		string accelerator;

		[Editor (typeof (Stetic.Editor.Accelerator))]
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

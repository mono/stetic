using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {
	public class ClassDescriptor {

		Type wrapped, wrapper;
		GLib.GType gtype;

		MethodInfo ctor_minfo;
		ConstructorInfo cinfo;
		bool use_gtype_ctor;

		string label, category;
		Gdk.Pixbuf icon;
		bool deprecated, hexpandable, vexpandable;

		ItemGroup internal_props = ItemGroup.Empty;
		ItemGroup commands = ItemGroup.Empty;
		ItemGroup contextMenu = ItemGroup.Empty;
		ArrayList groups = new ArrayList ();

		public ClassDescriptor (Assembly assembly, XmlElement elem)
		{
			wrapped = Type.GetType (elem.GetAttribute ("type"), true);
			wrapper = Type.GetType (elem.GetAttribute ("wrapper"), true);
			gtype = (GLib.GType)wrapped;

			label = elem.GetAttribute ("label");
			category = elem.GetAttribute ("palette-category");

			string iconname = elem.GetAttribute ("icon");
			try {
				icon = new Gdk.Pixbuf (assembly, iconname);
			} catch {
				icon = Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, 16, 0);
			}

			if (elem.HasAttribute ("deprecated"))
				deprecated = true;
			if (elem.HasAttribute ("hexpandable"))
				hexpandable = true;
			if (elem.HasAttribute ("vexpandable"))
				vexpandable = true;

			ctor_minfo = wrapper.GetMethod ("CreateInstance",
							BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
							null, new Type[0], null);
			if (ctor_minfo == null) {
				cinfo = wrapped.GetConstructor (new Type[0]);
				if (cinfo == null) {
					use_gtype_ctor = true;
					cinfo = wrapped.GetConstructor (new Type[] { typeof (IntPtr) });
				}
			}

			XmlElement internal_elem = elem["internal-properties"];
			if (internal_elem != null)
				internal_props = new ItemGroup (internal_elem, this);

			XmlElement commands_elem = elem["commands"];
			if (commands_elem != null)
				commands = new ItemGroup (commands_elem, this);

			XmlElement groups_elem = elem["itemgroups"];
			if (groups_elem != null) {
				foreach (XmlElement group_elem in groups_elem.SelectNodes ("./itemgroup")) {
					ItemGroup itemgroup;

					if (group_elem.HasAttribute ("ref")) {
						string refname = group_elem.GetAttribute ("ref");
						itemgroup = Registry.LookupItemGroup (refname);
					} else
						itemgroup = new ItemGroup (group_elem, this);
					groups.Add (itemgroup);
				}
			}

			XmlElement context_elem = elem["contextmenu"];
			if (context_elem != null)
				contextMenu = new ItemGroup (context_elem, this);
			else
				contextMenu = ItemGroup.Empty;
		}

		public Type WrappedType {
			get {
				return wrapped;
			}
		}

		public Type WrapperType {
			get {
				return wrapper;
			}
		}

		public GLib.GType GType {
			get {
				return gtype;
			}
		}

		public string CName {
			get {
				return gtype.ToString ();
			}
		}

		public bool Deprecated {
			get {
				return deprecated;
			}
		}

		public bool HExpandable {
			get {
				return hexpandable;
			}
		}

		public bool VExpandable {
			get {
				return vexpandable;
			}
		}

		public string Label {
			get {
				return label;
			}
		}

		public Gdk.Pixbuf Icon {
			get {
				return icon;
			}
		}

		public string Category {
			get {
				return category;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		public object NewInstance (IStetic stetic)
		{
			object inst;

			if (ctor_minfo != null)
				inst = ctor_minfo.Invoke (null, new object[0]);
			else if (!use_gtype_ctor)
				inst = cinfo.Invoke (null, new object[0]);
			else {
				IntPtr raw = g_object_new (gtype.Val, IntPtr.Zero);
				inst = cinfo.Invoke (new object[] { raw });
			}

			ObjectWrapper.Create (stetic, inst, false);
			return inst;
		}

		public ItemDescriptor this[string name] {
			get {
				ItemDescriptor item;

				item = internal_props[name];
				if (item != null)
					return item;
				item = commands[name];
				if (item != null)
					return item;

				if (groups != null) {
					foreach (ItemGroup group in groups) {
						item = group[name];
						if (item != null)
							return item;
					}
				}

				return null;
			}
		}

		public ArrayList ItemGroups {
			get {
				return groups;
			}
		}

		public ItemGroup ContextMenu {
			get {
				return contextMenu;
			}
		}
	}
}

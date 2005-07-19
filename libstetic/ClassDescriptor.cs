using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {
	public class ClassDescriptor {

		Type wrapped, wrapper;
		GLib.GType gtype;

		MethodInfo ctorMethodInfo;
		ConstructorInfo cinfo;
		bool useGTypeCtor;

		string label, category, cname;
		Gdk.Pixbuf icon;
		bool deprecated, hexpandable, vexpandable;

		ArrayList groups = new ArrayList ();
		int importantGroups;
		ItemGroup contextMenu;
		ItemGroup internalChildren;

		public ClassDescriptor (Assembly assembly, XmlElement elem)
		{
			wrapped = Type.GetType (elem.GetAttribute ("type"), true);
			if (elem.HasAttribute ("wrapper"))
			    wrapper = Type.GetType (elem.GetAttribute ("wrapper"), true);
			else {
				for (Type type = wrapped.BaseType; type != null; type = type.BaseType) {
					ClassDescriptor parent = Registry.LookupClass (type);
					if (parent != null) {
						wrapper = parent.WrapperType;
						break;
					}
				}
				if (wrapper == null)
					throw new ArgumentException ("No wrapper type for class {0}", wrapped.FullName);
			}

			gtype = (GLib.GType)wrapped;
			if (elem.HasAttribute ("cname"))
				cname = elem.GetAttribute ("cname");
			else
				cname = gtype.ToString ();

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

			ctorMethodInfo = wrapper.GetMethod ("CreateInstance",
							    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
							    null, new Type[0], null);
			if (ctorMethodInfo == null) {
				cinfo = wrapped.GetConstructor (new Type[0]);
				if (cinfo == null) {
					useGTypeCtor = true;
					cinfo = wrapped.GetConstructor (new Type[] { typeof (IntPtr) });
				}
			}

			XmlElement groupsElem = elem["itemgroups"];
			if (groupsElem != null) {
				foreach (XmlElement groupElem in groupsElem.SelectNodes ("itemgroup")) {
					ItemGroup itemgroup;

					if (groupElem.HasAttribute ("ref")) {
						string refname = groupElem.GetAttribute ("ref");
						itemgroup = Registry.LookupItemGroup (refname);
					} else
						itemgroup = new ItemGroup (groupElem, this);
					groups.Add (itemgroup);

					if (groupElem.HasAttribute ("important")) {
						if (groupElem.GetAttribute ("important") == "true")
							importantGroups++;
					} else if (groups.Count == 1)
						importantGroups++;
				}
			}

			XmlElement contextElem = elem["contextmenu"];
			if (contextElem != null)
				contextMenu = new ItemGroup (contextElem, this);
			else
				contextMenu = ItemGroup.Empty;

			XmlElement ichildElem = elem["internal-children"];
			if (ichildElem != null)
				internalChildren = new ItemGroup (ichildElem, this);
			else
				internalChildren = ItemGroup.Empty;
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
				return cname;
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

		int counter;

		public object NewInstance (IProject proj)
		{
			object inst;

			if (ctorMethodInfo != null)
				inst = ctorMethodInfo.Invoke (null, new object[0]);
			else if (!useGTypeCtor)
				inst = cinfo.Invoke (null, new object[0]);
			else {
				IntPtr raw = g_object_new (gtype.Val, IntPtr.Zero);
				inst = cinfo.Invoke (new object[] { raw });
			}

			string name = wrapped.Name.ToLower () + (++counter).ToString ();
			foreach (ItemGroup group in groups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop != null && prop.InitWithName)
						prop.SetValue (inst, name);
				}
			}

			ObjectWrapper.Create (proj, inst, false);
			return inst;
		}

		public ItemDescriptor this[string name] {
			get {
				if (groups != null) {
					foreach (ItemGroup group in groups) {
						ItemDescriptor item = group[name];
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

		public int ImportantGroups {
			get {
				return importantGroups;
			}
		}

		public ItemGroup ContextMenu {
			get {
				return contextMenu;
			}
		}

		public ItemGroup InternalChildren {
			get {
				return internalChildren;
			}
		}
	}
}

using System;
using System.Collections;

// Don't warn that OptionMenu is deprecated. We know that.
#pragma warning disable 612

namespace Stetic.Wrapper {

	[ObjectWrapper ("Option Menu", "optionmenu.png", ObjectWrapperType.Widget, Deprecated = true)]
	public class OptionMenu : Container {

		public static new Type WrappedType = typeof (Gtk.OptionMenu);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Option Menu Properties",
				      "Items",
				      "Active");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (optionmenu.Menu == null) {
				Gtk.Menu menu = new Gtk.Menu ();
				menu.Show ();
				optionmenu.Menu = menu;
			}
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			GladeUtils.ExtractProperty ("history", props);
			base.GladeImport (className, id, props);
			stetic.GladeImportComplete += FlattenMenu;
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
			if (wrapper == null)
				return null;

			Gtk.Menu menu = (Gtk.Menu)wrapper.Wrapped;
			if (menu != null)
				optionmenu.Menu = menu;

			return wrapper as Stetic.Wrapper.Widget;
		}

		void FlattenMenu ()
		{
			Gtk.Menu menu = optionmenu.Menu as Gtk.Menu;
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();

			foreach (Gtk.Widget w in menu.Children) {
				Gtk.MenuItem item = w as Gtk.MenuItem;
				if (item == null)
					continue;
				Gtk.Label label = item.Child as Gtk.Label;
				if (label != null)
					sb.Append (label.LabelProp);
				sb.Append ("\n");
			}

			Items = sb.ToString ();
		}

		Gtk.OptionMenu optionmenu {
			get {
				return (Gtk.OptionMenu)Wrapped;
			}
		}

		string items = "";
		string[] item = new string[0];

		[Editor (typeof (Stetic.Editor.Text))]
		[Description ("Items", "The items to display in the Option Menu, one per line")]
		public string Items {
			get {
				return items;
			}
			set {
				int active = optionmenu.History;
				Gtk.Menu menu = new Gtk.Menu ();

				items = value;
				while (items.EndsWith ("\n"))
					items = items.Substring (0, items.Length - 1);
				item = items.Split ('\n');

				for (int i = 0; i < item.Length; i++) {
					Gtk.MenuItem mitem;
					if (item[i] == "")
						mitem = new Gtk.SeparatorMenuItem ();
					else
						mitem = new Gtk.MenuItem (item[i]);
					mitem.Show ();
					mitem.Activated += delegate (object obj, EventArgs args) {
						EmitNotify ("Active");
					};
					menu.Add (mitem);
				}

				menu.Show ();
				optionmenu.Menu = menu;
				optionmenu.SetHistory ((uint)active);

				EmitNotify ("Items");
			}
		}

		[Description ("Active", "The active menu item")]
		[Range (0, System.Int32.MaxValue)]
		public int Active {
			get {
				return optionmenu.History;
			}
			set {
				optionmenu.SetHistory ((uint)value);
				EmitNotify ("Active");
			}
		}
	}
}

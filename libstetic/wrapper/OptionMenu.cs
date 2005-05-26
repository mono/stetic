using System;
using System.Collections;

// Don't warn that OptionMenu is deprecated. We know that.
#pragma warning disable 612

namespace Stetic.Wrapper {

	[ObjectWrapper ("Option Menu", "optionmenu.png", ObjectWrapperType.Widget, Deprecated = true)]
	public class OptionMenu : Container {

		public static new Type WrappedType = typeof (Gtk.OptionMenu);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Option Menu Properties",
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
			Widget menuWrapper = (Widget)ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Menu), optionmenu.Menu);
			menuWrapper.InternalChildId = "menu";
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			string history = GladeUtils.ExtractProperty ("history", props);
			base.GladeImport (className, id, props);
			stetic.GladeImportComplete += delegate () {
				Gtk.Widget menu = optionmenu.Menu;
				optionmenu.Menu = new Gtk.Menu ();
				optionmenu.Menu = menu;
				if (history != null)
					Active = Int32.Parse (history);
				else
					Active = 0;
			};
		}

		// Some versions of glade call the menu an internal child, some don't

		public override Widget GladeSetInternalChild (string childId, string className, string id, Hashtable props)
		{
			if (childId != "menu")
				return base.GladeSetInternalChild (childId, className, id, props);

			Widget wrapper = Stetic.Wrapper.Widget.Lookup (optionmenu.Menu);
			GladeUtils.ImportWidget (stetic, wrapper, wrapper.Wrapped, id, props);
			return wrapper;
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			return GladeSetInternalChild ("menu", className, id, props);
		}

		public override IEnumerable GladeChildren {
			get {
				return new Gtk.Widget[] { optionmenu.Menu };
			}
		}

		Gtk.OptionMenu optionmenu {
			get {
				return (Gtk.OptionMenu)Wrapped;
			}
		}

		[GladeProperty (Name = "history")]
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

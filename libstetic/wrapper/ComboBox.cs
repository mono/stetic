using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box", "combo.png", ObjectWrapperType.Widget)]
	public class ComboBox : Widget {

		public static new Type WrappedType = typeof (Gtk.ComboBox);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Combo Box Properties",
				      "Items",
				      "Active");
			AddItemGroup (type, "Extra ComboBox Properties",
				      "WrapWidth",
				      "ColumnSpanColumn",
				      "RowSpanColumn");
		}

		public static new Gtk.ComboBox CreateInstance ()
		{
			return Gtk.ComboBox.NewText ();
		}

		string items = "";
		string[] item = new string[0];

		[Editor (typeof (Stetic.Editor.Text))]
		[Description ("Items", "The items to display in the Combo Box, one per line")]
		public string Items {
			get {
				return items;
			}
			set {
				Gtk.ComboBox combobox = (Gtk.ComboBox)Wrapped;
				string[] newitem = value.Split ('\n');
				int active = combobox.Active;

				int row = 0, oi = 0, ni = 0;
				while (oi < item.Length && ni < newitem.Length) {
					if (item[oi] == newitem[ni]) {
						oi++;
						ni++;
						row++;
					} else if (ni < newitem.Length - 1 &&
						   item[oi] == newitem[ni + 1]) {
						combobox.InsertText (row++, newitem[ni++]);
						if (active > row)
							active++;
					} else {
						combobox.RemoveText (row);
						if (active > row)
							active--;
						oi++;
					}
				}

				while (oi < item.Length) {
					combobox.RemoveText (row);
					oi++;
				}

				while (ni < newitem.Length)
					combobox.InsertText (row++, newitem[ni++]);

				items = value;
				item = newitem;
				combobox.Active = active;

				EmitNotify ("Items");
			}
		}
	}
}

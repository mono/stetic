using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box", "combo.png", ObjectWrapperType.Widget)]
	public class ComboBox : Stetic.Wrapper.Widget {

		public static ItemGroup ComboBoxProperties;
		public static ItemGroup ComboBoxExtraProperties;

		static ComboBox () {
			ComboBoxProperties = new ItemGroup ("Combo Box Properties",
							    typeof (Stetic.Wrapper.ComboBox),
							    typeof (Gtk.ComboBox),
							    "Items",
							    "Active");
			ComboBoxExtraProperties = new ItemGroup ("Extra ComboBox Properties",
								 typeof (Gtk.ComboBox),
								 "WrapWidth",
								 "ColumnSpanColumn",
								 "RowSpanColumn");
			RegisterWrapper (typeof (Stetic.Wrapper.ComboBox),
					 ComboBoxProperties,
					 ComboBoxExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public ComboBox (IStetic stetic) : this (stetic, Gtk.ComboBox.NewText ()) {}

		public ComboBox (IStetic stetic, Gtk.ComboBox combobox) : base (stetic, combobox)
		{
			items = "";
			item = new string[0];
		}

		string items;
		string[] item;

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

using System;

namespace Stetic.Wrapper {

	public class Dialog : Window {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			dialog.HasSeparator = false;

			if (!initialized && dialog.VBox.Children.Length == 1) {
				Placeholder ph = CreatePlaceholder ();
				ph.SetSizeRequest (200, 200);
				dialog.VBox.Add (ph);
				Buttons = StandardButtons.Close;
			}
		}

		Gtk.Dialog dialog {
			get {
				return (Gtk.Dialog)Wrapped;
			}
		}

		public enum StandardButtons {
			Custom,
			Close,
			Ok,
			CancelOk,
		}

		StandardButtons buttons;

		public StandardButtons Buttons {
			get {
				return buttons;
			}
			set {
				if (buttons == value)
					return;
				buttons = value;

				Gtk.ButtonBox actionArea = dialog.ActionArea;

				Gtk.Widget[] children = actionArea.Children;
				foreach (Gtk.Widget w in children) {
					if (w != helpButton)
						w.Destroy ();
				}

				switch (buttons) {
				case StandardButtons.Close:
					AddButton (Gtk.Stock.Close, Gtk.ResponseType.Close, true);
					break;

				case StandardButtons.Ok:
					AddButton (Gtk.Stock.Ok, Gtk.ResponseType.Ok, true);
					break;

				case StandardButtons.CancelOk:
					AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, false);
					AddButton (Gtk.Stock.Ok, Gtk.ResponseType.Ok, true);
					break;

				case StandardButtons.Custom:
					AddButton (null, 0, true);
					break;
				}
			}
		}

		Gtk.Button helpButton;

		public bool HelpButton {
			get {
				return helpButton != null;
			}
			set {
				if (HelpButton == value)
					return;

				if (value) {
					helpButton = AddButton (Gtk.Stock.Help, Gtk.ResponseType.Help, false);
				} else {
					helpButton.Destroy ();
					helpButton = null;
				}
			}
		}

		static ClassDescriptor buttonClass;

		Gtk.Button AddButton (string stockId, Gtk.ResponseType response, bool hasDefault)
		{
			Stetic.Wrapper.Button wrapper;
			Gtk.Button button;

			if (buttonClass == null)
				buttonClass = Registry.LookupClass ("GtkButton");

			button = (Gtk.Button)buttonClass.NewInstance (proj);
			wrapper = (Stetic.Wrapper.Button) ObjectWrapper.Lookup (button);
			if (stockId != null) {
				wrapper.Type = Button.ButtonType.StockItem;
				wrapper.StockId = stockId;
			} else {
				wrapper.Type = Button.ButtonType.TextOnly;
				wrapper.Label = button.Name;
			}
			wrapper.ResponseId = (int)response;

			Stetic.Wrapper.Container actionArea = Stetic.Wrapper.Container.Lookup (dialog.ActionArea);
			actionArea.Add (button);

			button.CanDefault = true;
			wrapper.HasDefault = hasDefault;

			if (stockId == Gtk.Stock.Help)
				((Gtk.ButtonBox)actionArea.Wrapped).SetChildSecondary (button, true);

			return button;
		}
	}
}

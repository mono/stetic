using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Dialog Box", "dialog.png", ObjectWrapperType.Window)]
	public class Dialog : Window {

		public static new Type WrappedType = typeof (Gtk.Dialog);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Dialog Properties",
				      "Title",
				      "Buttons",
				      "HelpButton",
				      "Icon",
				      "WindowPosition",
				      "Modal",
				      "BorderWidth");
			AddItemGroup (type, "Miscellaneous Dialog Properties",
				      "HasSeparator",
				      "AcceptFocus",
				      "Decorated",
				      "DestroyWithParent",
				      "Gravity",
				      "Role",
				      "SkipPagerHint",
				      "SkipTaskbarHint");
		}

		public override void Wrap (object obj, bool initialized)
		{
			Stetic.Wrapper.Widget wrapper;

			base.Wrap (obj, initialized);
			dialog.HasSeparator = false;

			wrapper = ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.VBox), dialog.VBox) as Stetic.Wrapper.Widget;
			wrapper.InternalChildId = "vbox";
			if (dialog.VBox.Name == "GtkVBox")
				dialog.VBox.Name = dialog.Name + "_vbox";

			wrapper = ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.HButtonBox), dialog.ActionArea) as Stetic.Wrapper.Widget;
			wrapper.InternalChildId = "action_area";
			if (dialog.ActionArea.Name == "GtkHButtonBox")
				dialog.ActionArea.Name = dialog.Name + "_action_area";

			if (!initialized) {
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

//		[Editor (typeof (Stetic.Editor.DialogButtons))]
		[Description ("Buttons", "The buttons to display")]
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

		[Description ("Help Button", "Whether or not to display a \"Help\" button")]
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

		Gtk.Button AddButton (string stockId, Gtk.ResponseType response, bool hasDefault)
		{
			Stetic.Wrapper.Button wrapper;
			Gtk.Button button;

			wrapper = ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Button)) as Stetic.Wrapper.Button;
			button = (Gtk.Button)wrapper.Wrapped;
			if (stockId != null)
				wrapper.Icon = "stock:" + stockId;
			else {
				wrapper.Icon = null;
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

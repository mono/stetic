using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Message Dialog", "messagedialog.png", ObjectWrapperType.Window)]
	public class MessageDialog : Dialog {

		public static ItemGroup MessageDialogProperties;

		static MessageDialog () {
			MessageDialogProperties = new ItemGroup ("Message Dialog Properties",
								 typeof (Stetic.Wrapper.MessageDialog),
								 typeof (Gtk.MessageDialog),
								 "Title",
								 "MessageType",
								 "PrimaryText",
								 "SecondaryText",
								 "Buttons",
								 "Icon",
								 "WindowPosition",
								 "Modal",
								 "BorderWidth");
			RegisterItems (typeof (Stetic.Wrapper.MessageDialog),
				       MessageDialogProperties,
				       Dialog.DialogMiscProperties,
				       Window.WindowSizeProperties,
				       Widget.CommonWidgetProperties);
		}

		Gtk.Label label, secondaryLabel;
		Gtk.Image icon;

		public MessageDialog (IStetic stetic) : this (stetic, new Gtk.Dialog ()) {}

		public MessageDialog (IStetic stetic, Gtk.Dialog dialog) : base (stetic, dialog)
		{
			dialog.Resizable = false;
			dialog.BorderWidth = 12;

			label = new Gtk.Label ();
			label.LineWrap = true;
			label.Selectable = true;
			label.UseMarkup = true;
			label.SetAlignment (0.0f, 0.0f);

			secondaryLabel = new Gtk.Label ();
			secondaryLabel.LineWrap = true;
			secondaryLabel.Selectable = true;
			secondaryLabel.UseMarkup = true;
			secondaryLabel.SetAlignment (0.0f, 0.0f);

			icon = new Gtk.Image (Gtk.Stock.DialogInfo, Gtk.IconSize.Dialog);
			icon.SetAlignment (0.5f, 0.0f);

			Gtk.HBox hbox = new Gtk.HBox (false, 12);
			Gtk.VBox vbox = new Gtk.VBox (false, 12);

			vbox.PackStart (label, false, false, 0);
			vbox.PackStart (secondaryLabel, true, true, 0);

			hbox.PackStart (icon, false, false, 0);
			hbox.PackStart (vbox, true, true, 0);

			dialog.VBox.PackStart (hbox, false, false, 0);
			hbox.ShowAll ();

			Buttons = Gtk.ButtonsType.OkCancel;
		}

		private Gtk.Dialog dialog {
			get {
				return (Gtk.Dialog)Wrapped;
			}
		}

		public Gtk.MessageType MessageType {
			get {
				if (icon.Stock == Gtk.Stock.DialogInfo)
					return Gtk.MessageType.Info;
				else if (icon.Stock == Gtk.Stock.DialogQuestion)
					return Gtk.MessageType.Question;
				else if (icon.Stock == Gtk.Stock.DialogWarning)
					return Gtk.MessageType.Warning;
				else
					return Gtk.MessageType.Error;
			}
			set {
				Gtk.StockItem item = Gtk.Stock.Lookup (icon.Stock);
				bool setTitle = (dialog.Title == "") || (dialog.Title == item.Label);

				if (value == Gtk.MessageType.Info)
					icon.Stock = Gtk.Stock.DialogInfo;
				else if (value == Gtk.MessageType.Question)
					icon.Stock = Gtk.Stock.DialogQuestion;
				else if (value == Gtk.MessageType.Warning)
					icon.Stock = Gtk.Stock.DialogWarning;
				else
					icon.Stock = Gtk.Stock.DialogError;

				if (setTitle) {
					item = Gtk.Stock.Lookup (icon.Stock);
					dialog.Title = item.Label;
				}
			}
		}

		public string primaryText;
		public string PrimaryText {
			get {
				return primaryText;
			}
			set {
				primaryText = value;
				label.Markup = "<b>" + value + "</b>";
			}
		}

		public string SecondaryText {
			get {
				return secondaryLabel.Text;
			}
			set {
				secondaryLabel.Markup = value;
			}
		}

		Gtk.ButtonsType buttons;
		public Gtk.ButtonsType Buttons {
			get {
				return buttons;
			}
			set {
				Gtk.Widget[] oldButtons = dialog.ActionArea.Children;
				foreach (Gtk.Widget w in oldButtons)
					dialog.ActionArea.Remove (w);

				buttons = value;
				switch (buttons) {
				case Gtk.ButtonsType.None:
					// nothing
					break;

				case Gtk.ButtonsType.Ok:
					dialog.AddButton (Gtk.Stock.Ok, Gtk.ResponseType.Ok);
					break;

				case Gtk.ButtonsType.Close:
					dialog.AddButton (Gtk.Stock.Close, Gtk.ResponseType.Close);
					break;

				case Gtk.ButtonsType.Cancel:
					dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
					break;

				case Gtk.ButtonsType.YesNo:
					dialog.AddButton (Gtk.Stock.No, Gtk.ResponseType.No);
					dialog.AddButton (Gtk.Stock.Yes, Gtk.ResponseType.Yes);
					break;

				case Gtk.ButtonsType.OkCancel:
					dialog.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
					dialog.AddButton (Gtk.Stock.Ok, Gtk.ResponseType.Ok);
					break;
				}
			}
		}
	}
}

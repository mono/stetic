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
			WidgetSite site;

			base.Wrap (obj, initialized);
			dialog.HasSeparator = false;

			site = CreateWidgetSite ();
			site.InternalChildId = "vbox";
			dialog.VBox.Reparent (site);
			dialog.Add (site);
			ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.VBox), dialog.VBox);
			if (dialog.VBox.Name == "GtkVBox")
				dialog.VBox.Name = dialog.Name + "_vbox";

			site = CreateWidgetSite ();
			site.InternalChildId = "action_area";
			dialog.ActionArea.Reparent (site);
			dialog.VBox.PackEnd (site, false, true, 0);
			ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.HButtonBox), dialog.ActionArea);
			if (dialog.ActionArea.Name == "GtkHButtonBox")
				dialog.ActionArea.Name = dialog.Name + "_action_area";

			if (!initialized) {
				site = CreateWidgetSite ();
				Gtk.Requisition req;
				req.Width = req.Height = 200;
				site.EmptySize = req;
				dialog.VBox.Add (site);
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

		WidgetSite helpButton;

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

		WidgetSite AddButton (string stockId, Gtk.ResponseType response, bool hasDefault)
		{
			Stetic.Wrapper.Button button;
			Gtk.Button widget;

			button = ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Button)) as Stetic.Wrapper.Button;
			widget = (Gtk.Button)button.Wrapped;
			if (stockId != null)
				button.Icon = "stock:" + stockId;
			else {
				button.Icon = null;
				button.Label = widget.Name;
			}
			button.ResponseId = (int)response;

			Stetic.Wrapper.Container actionArea = Stetic.Wrapper.Container.Lookup (dialog.ActionArea);
			WidgetSite site = actionArea.AddPlaceholder ();
			site.Add (widget);

			widget.CanDefault = true;
			button.HasDefault = hasDefault;

			if (stockId == Gtk.Stock.Help)
				((Gtk.ButtonBox)actionArea.Wrapped).SetChildSecondary (site, true);

			return site;
		}
	}
}

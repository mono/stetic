using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Button : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			if (button.UseStock) {
				type = ButtonType.StockItem;
				StockId = button.Label;
			} else if (!initialized) {
				type = ButtonType.TextOnly;
				Label = button.Name;
			} else if (button.Child is Gtk.Label) {
				type = ButtonType.TextOnly;
				label = button.Label;
				useUnderline = button.UseUnderline;
			} else {
				type = ButtonType.Custom;
				FixupGladeChildren ();
			}
		}

		public override void GladeImport (XmlElement elem)
		{
			base.GladeImport (elem);
		}

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			Type = ButtonType.Custom;

			if (button.Child != null)
				button.Remove (button.Child);
			Widget wrapper = base.GladeImportChild (child_elem);
			FixupGladeChildren ();
			return wrapper;
		}

		void FixupGladeChildren ()
		{
			Gtk.Alignment alignment = button.Child as Gtk.Alignment;
			if (alignment == null)
				return;
			Gtk.HBox box = alignment.Child as Gtk.HBox;
			if (box == null)
				return;

			Gtk.Widget[] children = box.Children;
			if (children == null || children.Length != 2)
				return;

			Gtk.Image image = children[0] as Gtk.Image;
			Gtk.Label label = children[1] as Gtk.Label;
			if (image == null || label == null)
				return;
			Stetic.Wrapper.Image iwrap = Stetic.ObjectWrapper.Lookup (image) as Stetic.Wrapper.Image;
			if (iwrap == null)
				return;

			this.label = label.LabelProp;
			button.UseUnderline = label.UseUnderline;

			if (iwrap.Type == Image.ImageType.ThemedIcon) {
				themedIcon = iwrap.IconName;
				Type = ButtonType.ThemedIcon;
			} else {
				applicationIcon = iwrap.Filename;
				Type = ButtonType.ApplicationIcon;
			}
		}

		public override XmlElement GladeExport (XmlDocument doc)
		{
			XmlElement elem = base.GladeExport (doc);
			if (Type == ButtonType.StockItem)
				GladeUtils.SetProperty (elem, "label", stockId);
			return elem;
		}

		public override IEnumerable RealChildren {
			get {
				if (type == ButtonType.Custom)
					return base.RealChildren;
				else
					return new Gtk.Widget[0];
			}
		}

		public override IEnumerable GladeChildren {
			get {
				if (type == ButtonType.StockItem || type == ButtonType.TextOnly)
					return new Gtk.Widget[0];
				else
					return base.GladeChildren;
			}
		}

		private Gtk.Button button {
			get {
				return (Gtk.Button)Wrapped;
			}
		}

		public enum ButtonType {
			StockItem,
			TextOnly,
			ThemedIcon,
			ApplicationIcon,
			Custom
		};

		ButtonType type;
		public ButtonType Type {
			get {
				return type;
			}
			set {
				type = value;
				EmitNotify ("Type");
				switch (type) {
				case ButtonType.StockItem:
					button.UseStock = true;
					StockId = stockId;
					break;
				case ButtonType.TextOnly:
					labelWidget = null;
					button.UseStock = false;
					Label = label;
					UseUnderline = useUnderline;
					break;
				case ButtonType.ThemedIcon:
					button.UseStock = false;
					Label = label;
					UseUnderline = useUnderline;
					ThemedIcon = themedIcon;
					break;
				case ButtonType.ApplicationIcon:
					button.UseStock = false;
					Label = label;
					UseUnderline = useUnderline;
					ApplicationIcon = applicationIcon;
					break;
				case ButtonType.Custom:
					button.UseStock = false;
					if (button.Child != null)
						ReplaceChild (button.Child, CreatePlaceholder ());
					break;
				}
			}
		}

		Gtk.Label labelWidget;

		void ConstructContents ()
		{
			if (button.Child != null)
				button.Remove (button.Child);

			if (useUnderline) {
				labelWidget = new Gtk.Label (label);
				labelWidget.MnemonicWidget = button;
			} else
				labelWidget = Gtk.Label.New (label);

			Gtk.Image imageWidget = (Gtk.Image)Registry.NewInstance (typeof (Gtk.Image), proj);
			Image imageWrapper = (Image)Widget.Lookup (imageWidget);
			imageWrapper.Unselectable = true;
			if (type == ButtonType.StockItem) {
				imageWrapper.IconName = stockId;
				imageWrapper.IconSize = Gtk.IconSize.Button;
				imageWrapper.Type = Image.ImageType.ThemedIcon;
			} else if (type == ButtonType.ThemedIcon) {
				imageWrapper.IconName = themedIcon;
				imageWrapper.IconSize = Gtk.IconSize.Button;
				imageWrapper.Type = Image.ImageType.ThemedIcon;
			} else {
				imageWrapper.Filename = applicationIcon;
				imageWrapper.Type = Image.ImageType.ApplicationImage;
			}

			Gtk.HBox box = new Gtk.HBox (false, 2);
			box.PackStart (imageWidget, false, false, 0);
			box.PackEnd (labelWidget, false, false, 0);

			Gtk.Alignment alignment = new Gtk.Alignment (button.Xalign, button.Yalign, 0.0f, 0.0f);
			alignment.Add (box);

			Widget wrapper = (Widget)ObjectWrapper.Create (proj, labelWidget);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (proj, box);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (proj, alignment);
			wrapper.Unselectable = true;

			alignment.ShowAll ();
			button.Add (alignment);
		}

		string stockId = Gtk.Stock.Ok;
		public string StockId {
			get {
				return stockId;
			}
			set {
				if (responseId == ResponseIdForStockId (stockId))
					responseId = 0;

				button.Label = stockId = value;
				button.UseStock = true;
				Gtk.StockItem item = Gtk.Stock.Lookup (value);
				if (item.StockId == value) {
					label = item.Label;
					useUnderline = true;
				}
				EmitNotify ("StockId");

				if (responseId == 0)
					ResponseId = ResponseIdForStockId (stockId);
			}
		}

		string applicationIcon;
		public string ApplicationIcon {
			get {
				return applicationIcon;
			}
			set {
				applicationIcon = value;
				ConstructContents ();
				EmitNotify ("ApplicationIcon");
			}
		}

		string themedIcon;
		public string ThemedIcon {
			get {
				return themedIcon;
			}
			set {
				themedIcon = value;
				ConstructContents ();
				EmitNotify ("ThemedIcon");
			}
		}

		string label;
		public string Label {
			get {
				return label;
			}
			set {
				label = value;
				if (labelWidget != null)
					labelWidget.LabelProp = value;
				else
					button.Label = value;
			}
		}

		bool useUnderline;
		public bool UseUnderline {
			get {
				return useUnderline;
			}
			set {
				useUnderline = value;
				if (labelWidget != null)
					labelWidget.UseUnderline = value;
				else
					button.UseUnderline = value;
			}
		}

		bool isDialogButton;
		public bool IsDialogButton {
			get {
				return isDialogButton;
			}
			set {
				isDialogButton = value;
				if (isDialogButton)
					button.CanDefault = true;
			}
		}

		int responseId;
		public int ResponseId {
			get {
				return responseId;
			}
			set {
				responseId = value;
				EmitNotify ("ResponseId");
			}
		}

		int ResponseIdForStockId (string stockId)
		{
			if (stockId == Gtk.Stock.Ok)
				return (int)Gtk.ResponseType.Ok;
			else if (stockId == Gtk.Stock.Cancel)
				return (int)Gtk.ResponseType.Cancel;
			else if (stockId == Gtk.Stock.Close)
				return (int)Gtk.ResponseType.Close;
			else if (stockId == Gtk.Stock.Yes)
				return (int)Gtk.ResponseType.Yes;
			else if (stockId == Gtk.Stock.No)
				return (int)Gtk.ResponseType.No;
			else if (stockId == Gtk.Stock.Apply)
				return (int)Gtk.ResponseType.Apply;
			else if (stockId == Gtk.Stock.Help)
				return (int)Gtk.ResponseType.Help;
			else
				return 0;
		}
	}
}

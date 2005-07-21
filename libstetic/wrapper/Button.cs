using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Button : Container {

		public static new Gtk.Button CreateInstance ()
		{
			return new Gtk.Button (Gtk.Stock.Ok);
		}

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
			} else {
				type = ButtonType.Custom;
				FixupGladeChildren ();
			}
		}

		public override void GladeImport (XmlElement elem)
		{
			base.GladeImport (elem);
			if (elem.SelectSingleNode ("./property[@name='label']") == null &&
			    elem.SelectSingleNode ("./child/widget") == null)
				button.Remove (button.Child);
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
			GladeUtils.SetProperty (elem, "use_stock",
						(Type == ButtonType.StockItem) ? "True" : "False");
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
					return base.RealChildren;
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
					button.UseStock = false;
					button.Image = null;
					Label = label;
					break;
				case ButtonType.ThemedIcon:
					button.UseStock = false;
					Label = label;
					ThemedIcon = themedIcon;
					break;
				case ButtonType.ApplicationIcon:
					button.UseStock = false;
					Label = label;
					ApplicationIcon = applicationIcon;
					break;
				case ButtonType.Custom:
					button.UseStock = false;
					button.Image = null;
					if (button.Child != null)
						ReplaceChild (button.Child, CreatePlaceholder ());
					break;
				}
			}
		}

		void ConstructContents ()
		{
			Gtk.Label labelWidget;
			Gtk.Image iconWidget;

			if (button.Child != null)
				button.Remove (button.Child);

			if (button.UseUnderline) {
				labelWidget = new Gtk.Label (label);
				labelWidget.MnemonicWidget = button;
			} else
				labelWidget = Gtk.Label.New (label);

			if (type == ButtonType.ThemedIcon)
				iconWidget = Gtk.Image.NewFromIconName (themedIcon, Gtk.IconSize.Button);
			else
				iconWidget = new Gtk.Image (applicationIcon);

			Gtk.HBox box = new Gtk.HBox (false, 2);
			box.PackStart (iconWidget, false, false, 0);
			box.PackEnd (labelWidget, false, false, 0);

			Gtk.Alignment alignment = new Gtk.Alignment (button.Xalign, button.Yalign, 0.0f, 0.0f);
			alignment.Add (box);

			Widget wrapper = (Widget)ObjectWrapper.Create (proj, labelWidget);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (proj, iconWidget);
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
				button.Label = stockId = value;
				button.UseStock = true;
				Gtk.StockItem item = Gtk.Stock.Lookup (value);
				if (item.StockId == value)
					label = item.Label;
				EmitNotify ("StockId");
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
				button.Label = value;
			}
		}

		public bool HasResponseId {
			get {
				Stetic.Wrapper.Widget pwrap = ParentWrapper;
				if (pwrap == null)
					return false;
				return pwrap.InternalChildId == "action_area";
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
	}
}

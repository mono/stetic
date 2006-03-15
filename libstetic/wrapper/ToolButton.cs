using System;
using System.CodeDom;
using System.Xml;

namespace Stetic.Wrapper {

	public class ToolButton : Widget {

		ButtonType type;
		string stockId;
		string label;
		ImageInfo imageInfo;
		
		public enum ButtonType {
			StockItem,
			TextAndIcon
		};

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.ToolButton (Gtk.Stock.New);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			Gtk.ToolButton toolbutton = (Gtk.ToolButton)Wrapped;

			if (toolbutton.StockId != null) {
				stockId = toolbutton.StockId;
				type = ButtonType.StockItem;
			} else {
				type = ButtonType.TextAndIcon;
			}
		}

		public override void Read (XmlElement elem, FileFormat format)
		{
			if (format == FileFormat.Glade) {
				string icon = (string)GladeUtils.ExtractProperty (elem, "icon", "");
				stockId = (string)GladeUtils.ExtractProperty (elem, "stock_id", "");
				label = (string)GladeUtils.ExtractProperty (elem, "label", "");
				base.Read (elem, format);
				
				if (stockId != null && stockId.Length > 0) {
					Type = ButtonType.StockItem;
				} else if (icon != null && icon != "") {
					imageInfo = ImageInfo.FromFile (icon);
					Type = ButtonType.TextAndIcon;
				}
			} else
				base.Read (elem, format);
		}
		
		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			if (type != ButtonType.StockItem && imageInfo != null) {
				if (format == FileFormat.Glade) {
					switch (imageInfo.Source) {
						case ImageSource.File:
							GladeUtils.SetProperty (elem, "icon", imageInfo.Name);
							break;
						case ImageSource.Theme:
							GladeUtils.SetProperty (elem, "stock_id", imageInfo.Name);
							break;
						default:
							throw new System.NotSupportedException ("Image source not supported by Glade.");
					}
				}
			}
			return elem;
		}
		
		internal protected override CodeExpression GenerateWidgetCreation (GeneratorContext ctx)
		{
			return new CodeObjectCreateExpression (
				ClassDescriptor.WrappedTypeName, 
				new CodePrimitiveExpression (null), 
				new CodePrimitiveExpression (null)
			);
		}
		
		Gtk.ToolButton button {
			get { return (Gtk.ToolButton) Wrapped; }
		}
		
		public ButtonType Type {
			get {
				return type;
			}
			set {
				type = value;
				switch (type) {
				case ButtonType.StockItem:
					button.IconWidget = null;
					StockId = stockId;
					Label = label;
					break;
				case ButtonType.TextAndIcon:
					button.StockId = null;
					Icon = imageInfo;
					Label = label;
					break;
				}
				EmitNotify ("Type");
			}
		}
		
		public string Label {
			get { return label; }
			set {
				if (type == ButtonType.StockItem && value != null) {
					label = value.Length == 0 ? null : value;
				} else
					label = value;

				button.Label = label;
			}
		}

		public string StockId {
			get { return stockId; }
			set { 
				stockId = value;
				if (stockId != null && stockId.StartsWith ("stock:"))
					stockId = stockId.Substring (6);
				button.StockId = stockId;
				button.ShowAll ();
			}
		}

		public ImageInfo Icon {
			get { return imageInfo; }
			set {
				imageInfo = value;
				if (imageInfo != null) {
					button.IconWidget = new Gtk.Image (imageInfo.GetImage (Project));
					button.ShowAll ();
				}
				else
					button.IconWidget = null;
				EmitNotify ("Icon");
			}
		}
	}
}

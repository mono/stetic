using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static new Type WrappedType = typeof (Gtk.Image);

		internal static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Image Properties",
							"Type",
							"IconName",
							"IconSize",
							"Filename");
			props["IconName"].InvisibleIf (props["Type"], ImageType.ApplicationImage);
			props["IconSize"].InvisibleIf (props["Type"], ImageType.ApplicationImage);
			props["Filename"].InvisibleIf (props["Type"], ImageType.ThemedIcon);
		}

		public static new Gtk.Image CreateInstance ()
		{
			return new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (image.StorageType == Gtk.ImageType.Stock) {
				type = ImageType.ThemedIcon;
				iconName = image.Stock;
				iconSize = (Gtk.IconSize)image.IconSize;
			} else if (image.StorageType == Gtk.ImageType.IconName) {
				type = ImageType.ThemedIcon;
				iconName = image.IconName;
				iconSize = (Gtk.IconSize)image.IconSize;
			} else {
				type = ImageType.ApplicationImage;
			}
		}

		Gtk.Image image {
			get {
				return (Gtk.Image)Wrapped;
			}
		}

		public enum ImageType {
			ThemedIcon,
			ApplicationImage,
		}

		ImageType type;

		[Description ("Image Type", "Whether to use a themed icon or an application-provided image")]
		public ImageType Type {
			get {
				return type;
			}
			set {
				type = value;
				EmitNotify ("Type");

				if (type == ImageType.ThemedIcon) {
					IconName = iconName;
					IconSize = iconSize;
				} else
					Filename = filename;
			}
		}

		string iconName;

		[Editor (typeof (Stetic.Editor.ThemedIcon))]
		[Description ("Icon name", "The themed icon to display")]
		public string IconName {
			get {
				return iconName;
			}
			set {
				Gtk.StockItem item = Gtk.Stock.Lookup (value);
				if (item.StockId == value)
					image.Stock = iconName = value;
				else
					image.IconName = iconName = value;
			}
		}

		Gtk.IconSize iconSize;
		[GladeProperty (GladeProperty.UseUnderlying)]
		public Gtk.IconSize IconSize {
			get {
				return iconSize;
			}
			set {
				image.IconSize = (int)(iconSize = value);
			}
		}

		string filename;

		[Editor (typeof (Stetic.Editor.ImageFile))]
		[GladeProperty (Name = "pixbuf")]
		public string Filename {
			get {
				return filename;
			}
			set {
				if (value == "" || value == null) {
					image.Stock = Gtk.Stock.MissingImage;
					image.IconSize = (int)Gtk.IconSize.Button;
					filename = null;
				} else
					image.File = filename = value;
			}
		}
	}
}

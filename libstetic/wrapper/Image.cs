using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Image : Widget {

		public static new Gtk.Image CreateInstance ()
		{
			return new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			iconSize = (Gtk.IconSize)image.IconSize;

			if (image.StorageType == Gtk.ImageType.Stock) {
				iconName = image.Stock;
				Type = ImageType.ThemedIcon;
#if GTK_SHARP_2_6
			} else if (image.StorageType == Gtk.ImageType.IconName) {
				iconName = image.IconName;
				Type = ImageType.ThemedIcon;
#endif
			} else
				Type = ImageType.ApplicationImage;
		}

		Gtk.Image image {
			get {
				return (Gtk.Image)Wrapped;
			}
		}

		void BreakImage ()
		{
			image.IconSize = (int)Gtk.IconSize.Button;
			image.Stock = Gtk.Stock.MissingImage;
		}

		public enum ImageType {
			ThemedIcon,
			ApplicationImage,
		}

		ImageType type;
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
		public string IconName {
			get {
				return iconName;
			}
			set {
				iconName = value;
				EmitNotify ("IconName");

				if (value == null) {
					BreakImage ();
					return;
				}

				if (type != ImageType.ThemedIcon)
					type = ImageType.ThemedIcon;

				Gtk.StockItem item = Gtk.Stock.Lookup (iconName);
				if (item.StockId == iconName)
					image.Stock = iconName;
#if GTK_SHARP_2_6
				else
					image.IconName = iconName;
#endif
				IconSize = iconSize;
			}
		}

		Gtk.IconSize iconSize;
		public Gtk.IconSize IconSize {
			get {
				return iconSize;
			}
			set {
				iconSize = value;
				EmitNotify ("IconSize");

				if (iconName == null)
					return;

#if !GTK_SHARP_2_6
				if (image.StorageType != Gtk.ImageType.Stock) {
					bool ok = false;

					try {
						int w, h;
						Gtk.Icon.SizeLookup (iconSize, out w, out h);
						image.Pixbuf = Gtk.IconTheme.Default.LoadIcon (iconName, h, 0);
						ok = true;
					} catch {}

					if (!ok)
						BreakImage ();
				} else
#endif
					image.IconSize = (int)iconSize;
			}
		}

		string filename;
		public string Filename {
			get {
				return filename;
			}
			set {
				if (value == "" || value == null) {
					BreakImage ();
					filename = null;
				} else
					image.File = filename = value;

				EmitNotify ("Filename");
			}
		}
	}
}

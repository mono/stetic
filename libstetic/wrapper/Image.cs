using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static new Type WrappedType = typeof (Gtk.Image);

		static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Image Properties",
							"UseStock",
							"Stock",
							"IconSize",
							"File");
			props["Stock"].DependsOn (props["UseStock"]);			
			props["IconSize"].DependsOn (props["UseStock"]);			
			props["File"].DependsInverselyOn (props["UseStock"]);			
		}

		public static new Gtk.Image CreateInstance ()
		{
			return new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog);
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			useStock = (image.StorageType == Gtk.ImageType.Stock);
			if (useStock) {
				stock = Stock;
				iconSize = IconSize;
			}
		}

		Gtk.Image image {
			get {
				return (Gtk.Image)Wrapped;
			}
		}

		bool useStock;

		[Description ("Use Stock Icon", "Whether to use a stock icon rather than an image file")]
		public bool UseStock {
			get {
				return useStock;
			}
			set {
				useStock = value;
				EmitNotify ("UseStock");

				if (useStock) {
					Stock = stock;
					IconSize = iconSize;
				} else
					File = filename;
			}
		}

		string stock;

		[Editor (typeof (Stetic.Editor.StockItem))]
		[Description ("Stock Icon", "The stock icon to display")]
		public string Stock {
			get {
				return image.Stock;
			}
			set {
				image.Stock = stock = value;
			}
		}

		Gtk.IconSize iconSize;
		public Gtk.IconSize IconSize {
			get {
				return (Gtk.IconSize)image.IconSize;
			}
			set {
				image.IconSize = (int)(iconSize = value);
			}
		}

		string filename = "";

		[Editor (typeof (Stetic.Editor.File))]
		public string File {
			get {
				return filename;
			}
			set {
				image.File = filename = value;
			}
		}
	}
}

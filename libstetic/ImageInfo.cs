
using System;
using System.IO;
using System.CodeDom;

namespace Stetic
{
	public enum ImageSource
	{
		Theme,
		Resource,
		File
	}
	
	public class ImageInfo
	{
		ImageSource source;
		string name;
		Gtk.IconSize size;
		Gdk.Pixbuf image;
		
		private ImageInfo ()
		{
		}
		
		public string Label {
			get {
				if (source == ImageSource.File)
					return Path.GetFileName (name);
				else
					return name; 
			}
		}
		
		public string Name {
			get { return name; }
		}
		
		public Gtk.IconSize ThemeIconSize {
			get { return size; }
		}
		
		public ImageSource Source {
			get { return source; }
		}
		
		public override string ToString ()
		{
			if (source == ImageSource.Theme)
				return "stock:" + name + " " + size;
			else if (source == ImageSource.Resource)
				return "resource:" + name;
			else
				return "file:" + Path.GetFileName (name);
		}
		
		public static ImageInfo FromResource (string resourceName)
		{
			ImageInfo info = new ImageInfo ();
			info.name = resourceName;
			info.source = ImageSource.Resource;
			return info;
		}
		
		public static ImageInfo FromTheme (string iconId, Gtk.IconSize size)
		{
			ImageInfo info = new ImageInfo ();
			info.name = iconId;
			info.size = size;
			info.source = ImageSource.Theme;
			return info;
		}
		
		public static ImageInfo FromFile (string file)
		{
			ImageInfo info = new ImageInfo ();
			info.name = file;
			info.source = ImageSource.File;
			return info;
		}
		
		public static ImageInfo FromString (string str)
		{
			ImageInfo info = new ImageInfo ();
			if (str.StartsWith ("resource:")) {
				info.source = ImageSource.Resource;
				info.name = str.Substring (9);
			} else if (str.StartsWith ("stock:")) {
				info.source = ImageSource.Theme;
				string[] s = str.Substring (6).Split (' ');
				if (s.Length != 2)
					return null;
				info.name = s[0];
				info.size = (Gtk.IconSize) Enum.Parse (typeof(Gtk.IconSize), s[1]);
			} else if (str.StartsWith ("file:")) {
				info.source = ImageSource.File;
				info.name = str.Substring (5);
			} else
				return null;
			return info;
		}
		
		public Gdk.Pixbuf GetImage (IProject project)
		{
			if (image != null)
				return image;

			switch (source) {
				case ImageSource.Resource:
					if (project.ResourceProvider == null)
						return null;
					System.IO.Stream s = project.ResourceProvider.GetResourceStream (name);
					if (s == null)
						return null;
					try {
						return image = new Gdk.Pixbuf (s);
					} catch {
						// Not a valid image
						return Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, 16, 0);
					}
					
				case ImageSource.Theme:
					int w, h;
					Gtk.Icon.SizeLookup (size, out w, out h);
					Console.WriteLine ("IMAGE THEME n:" + name + " s:" + w);
					return image = Gtk.IconTheme.Default.LoadIcon (name, w, 0);
					
				case ImageSource.File:
					try {
						string file = name;
						if (!Path.IsPathRooted (name) && project.FileName != null) {
							file = Path.Combine (Path.GetDirectoryName (project.FileName), "pixmaps");
							file = Path.Combine (file, name);
						}
						Console.WriteLine ("IMAGE FILE:" + file);
						return image = new Gdk.Pixbuf (file);
					} catch {
						return Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, 16, 0);
					}
			}
			return null;
		}
		
		public Gdk.Pixbuf GetThumbnail (IProject project, int thumbnailSize)
		{
			Gdk.Pixbuf pix = GetImage (project);
			if (pix == null)
				return null;

			if (pix.Width > pix.Height) {
				if (pix.Width > thumbnailSize) {
					float prop = (float) pix.Height / (float) pix.Width;
					return pix.ScaleSimple (thumbnailSize, (int)(thumbnailSize * prop), Gdk.InterpType.Bilinear);
				}
			} else {
				if (pix.Height > thumbnailSize) {
					float prop = (float) pix.Width / (float) pix.Height;
					return pix.ScaleSimple ((int)(thumbnailSize * prop), thumbnailSize, Gdk.InterpType.Bilinear);
				}
			}
			return pix;
		}
		
		public CodeExpression ToCodeExpression ()
		{
			switch (source) {
				case ImageSource.Resource:
					return new CodeMethodInvokeExpression (
						new CodeTypeReferenceExpression (typeof(Gdk.Pixbuf)),
						"LoadFromResource",
						new CodePrimitiveExpression (name)
					);
					
				case ImageSource.Theme:
					int w, h;
					Gtk.Icon.SizeLookup (size, out w, out h);
					return new CodeMethodInvokeExpression (
						new CodePropertyReferenceExpression (
							new CodeTypeReferenceExpression (typeof(Gtk.IconTheme)), 
							"Default"
						),
						"LoadIcon",
						new CodePrimitiveExpression (name),
						new CodePrimitiveExpression (w),
						new CodePrimitiveExpression (0)
					);
					
				case ImageSource.File:
					return new CodeObjectCreateExpression (
						typeof(Gdk.Pixbuf),
						new CodePrimitiveExpression (name)
					);
			}
			return new CodePrimitiveExpression (null);
		}
	}
}

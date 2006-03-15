
using System;

namespace Stetic.Editor
{
	public class ImageSelector: Gtk.HBox, IPropertyEditor
	{
		Gtk.Image image;
		Gtk.Label entry;
		Gtk.Button button;
		Gtk.Button clearButton;
		ImageInfo icon;
		IProject project;
		Gtk.Frame imageFrame;
		
		public ImageSelector()
		{
			Spacing = 3;
			imageFrame = new Gtk.Frame ();
			imageFrame.Shadow = Gtk.ShadowType.In;
			imageFrame.BorderWidth = 2;
			PackStart (imageFrame, false, false, 0);

			image = new Gtk.Image (Gnome.Stock.Blank, Gtk.IconSize.Button);
			imageFrame.Add (image);
			
			Gtk.Frame frame = new Gtk.Frame ();
			entry = new Gtk.Label ();
			entry.Xalign = 0;
			frame.Shadow = Gtk.ShadowType.In;
			frame.BorderWidth = 2;
			frame.Add (entry);
			PackStart (frame, true, true, 0);

			clearButton = new Gtk.Button (new Gtk.Image (Gtk.Stock.Clear, Gtk.IconSize.Button));
			clearButton.Clicked += OnClearImage;
			PackStart (clearButton, false, false, 0);

			button = new Gtk.Button ();
			Gtk.Image icon = new Gtk.Image (Gtk.IconTheme.Default.LoadIcon ("stock_symbol-selection", 16, 0));
			button.Add (icon);
			PackStart (button, false, false, 0);
			button.Clicked += button_Clicked;
		}

		void button_Clicked (object obj, EventArgs args)
		{
			Gtk.Window parent = (Gtk.Window)GetAncestor (Gtk.Window.GType);
			using (SelectImageDialog dlg = new SelectImageDialog (parent, project)) {
				dlg.Icon = (ImageInfo) Value;
				if (dlg.Run () == (int) Gtk.ResponseType.Ok)
					Value = dlg.Icon;
			}
		}
		
		void OnClearImage (object obj, EventArgs args)
		{
			Value = null;
		}
		
		// Called once to initialize the editor.
		public void Initialize (PropertyDescriptor prop)
		{
			if (prop.PropertyType != typeof(ImageInfo))
				throw new ApplicationException ("ImageSelector editor does not support editing values of type " + prop.PropertyType);
		}
		
		// Called when the object to be edited changes.
		public void AttachObject (object obj)
		{
			Stetic.ObjectWrapper w = Stetic.ObjectWrapper.Lookup (obj);
			project = w.Project;
		}
		
		// Gets/Sets the value of the editor. If the editor supports
		// several value types, it is the responsibility of the editor 
		// to return values with the expected type.
		public object Value {
			get { return icon; }
			set {
				icon = (ImageInfo) value;
				if (icon != null) {
					entry.Text = icon.Label;
					image.Pixbuf = icon.GetThumbnail (project, 16);
					imageFrame.Show ();
					clearButton.Show ();
				} else {
					imageFrame.Hide ();
					clearButton.Hide ();
					entry.Text = "";
				}

				if (ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
			}
		}

		// To be fired when the edited value changes.
		public event EventHandler ValueChanged;
	}
}

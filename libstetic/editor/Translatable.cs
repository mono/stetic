using System;

namespace Stetic.Editor {

	public abstract class Translatable : Gtk.VBox {

		PropertyDescriptor prop;
		object obj;

		Gtk.Box mainHBox, contextBox, commentBox;
		Gtk.Button button;
		Gdk.Pixbuf globe, globe_not;
		Gtk.Image image;
		Gtk.Menu menu;
		Gtk.CheckMenuItem markItem;
		Gtk.MenuItem addContextItem, remContextItem, addCommentItem, remCommentItem;
		Gtk.Entry contextEntry;
		Stetic.TextBox commentText;

		public Translatable (PropertyDescriptor prop, object obj) : base (false, 3)
		{
			mainHBox = new Gtk.HBox (false, 6);
			PackStart (mainHBox, false, false, 0);

			if (!prop.Translatable)
				return;

			this.prop = prop;
			this.obj = obj;

			button = new Gtk.Button ();
			globe = Gdk.Pixbuf.LoadFromResource ("globe.png");
			globe_not = Gdk.Pixbuf.LoadFromResource ("globe-not.png");
			image = new Gtk.Image (prop.IsTranslated (obj) ? globe : globe_not);
			button.Add (image);
			button.ButtonPressEvent += ButtonPressed;
			mainHBox.PackEnd (button, false, false, 0);
			mainHBox.ShowAll ();

			menu = new Gtk.Menu ();

			markItem = new Gtk.CheckMenuItem ("Mark for Translation");
			markItem.Active = prop.IsTranslated (obj);
			markItem.Toggled += ToggleMark;
			markItem.Show ();
			menu.Add (markItem);

			addContextItem = new Gtk.MenuItem ("Add Translation Context Hint");
			addContextItem.Activated += AddContext;
			menu.Add (addContextItem);
			remContextItem = new Gtk.MenuItem ("Remove Translation Context Hint");
			remContextItem.Activated += RemoveContext;
			menu.Add (remContextItem);
			if (prop.IsTranslated (obj)) {
				if (prop.TranslationContext (obj) != null)
					remContextItem.Show ();
				else
					addContextItem.Show ();
			} else {
				addContextItem.Show ();
				addContextItem.Sensitive = false;
			}

			addCommentItem = new Gtk.MenuItem ("Add Comment for Translators");
			addCommentItem.Activated += AddComment;
			menu.Add (addCommentItem);
			remCommentItem = new Gtk.MenuItem ("Remove Comment for Translators");
			remCommentItem.Activated += RemoveComment;
			menu.Add (remCommentItem);
			if (prop.IsTranslated (obj)) {
				if (prop.TranslationComment (obj) != null)
					remCommentItem.Show ();
				else
					addCommentItem.Show ();
			} else {
				addCommentItem.Show ();
				addCommentItem.Sensitive = false;
			}

			contextBox = new Gtk.HBox (false, 6);
			Gtk.Label contextLabel = new Gtk.Label ("Translation context");
			contextLabel.Xalign = 0.0f;
			contextBox.PackStart (contextLabel, false, false, 0);
			contextEntry = new Gtk.Entry ();
			contextEntry.WidthChars = 8;
			contextBox.PackStart (contextEntry, true, true, 0);
			contextBox.ShowAll ();
			if (prop.TranslationContext (obj) != null) {
				PackStart (contextBox, false, false, 0);
				contextEntry.Text = prop.TranslationContext (obj);
			}
			contextEntry.Changed += ContextChanged;

			commentBox = new Gtk.VBox (false, 3);
			Gtk.Label commentLabel = new Gtk.Label ("Comment for Translators:");
			commentLabel.Xalign = 0.0f;
			commentBox.PackStart (commentLabel, false, false, 0);
			commentText = new Stetic.TextBox (3);
			commentBox.PackStart (commentText, false, false, 0);
			commentBox.ShowAll ();
			if (prop.TranslationComment (obj) != null) {
				PackEnd (commentBox, false, false, 0);
				commentText.Text = prop.TranslationComment (obj);
			}
			commentText.Changed += CommentChanged;
		}

		protected override void OnAdded (Gtk.Widget child)
		{
			mainHBox.PackStart (child, true, true, 0);
		}

		void MenuPosition (Gtk.Menu menu, out int x, out int y, out bool push_in)
		{
			button.GdkWindow.GetOrigin (out x, out y);
			Gdk.Rectangle alloc = button.Allocation;
			x += alloc.X;
			y += alloc.Y + alloc.Height;
			push_in = true;
		}

		[GLib.ConnectBefore]
		void ButtonPressed (object o, Gtk.ButtonPressEventArgs args)
		{
			menu.Popup (null, null, MenuPosition, IntPtr.Zero, 1, args.Event.Time);
			args.RetVal = true;
		}

		void ToggleMark (object o, EventArgs args)
		{
			if (!markItem.Active) {
				// Make sure we're showing the "Add" menu items
				// rather than the "Remove" ones
				if (prop.TranslationContext (obj) != null)
					RemoveContext (remContextItem, EventArgs.Empty);
				if (prop.TranslationComment (obj) != null)
					RemoveComment (remCommentItem, EventArgs.Empty);
			}

			prop.SetTranslated (obj, markItem.Active);
			image.Pixbuf = markItem.Active ? globe : globe_not;
			addContextItem.Sensitive = markItem.Active;
			addCommentItem.Sensitive = markItem.Active;
		}

		void AddContext (object o, EventArgs args)
		{
			prop.SetTranslationContext (obj, contextEntry.Text);
			PackStart (contextBox, false, false, 0);

			addContextItem.Hide ();
			remContextItem.Show ();
		}

		void RemoveContext (object o, EventArgs args)
		{
			prop.SetTranslationContext (obj, null);
			Remove (contextBox);

			remContextItem.Hide ();
			addContextItem.Show ();
		}

		void ContextChanged (object o, EventArgs args)
		{
			prop.SetTranslationContext (obj, contextEntry.Text);
		}

		void AddComment (object o, EventArgs args)
		{
			prop.SetTranslationComment (obj, commentText.Text);
			PackEnd (commentBox, false, false, 0);

			addCommentItem.Hide ();
			remCommentItem.Show ();
		}

		void RemoveComment (object o, EventArgs args)
		{
			prop.SetTranslationComment (obj, null);
			Remove (commentBox);

			remCommentItem.Hide ();
			addCommentItem.Show ();
		}

		void CommentChanged (object o, EventArgs args)
		{
			prop.SetTranslationComment (obj, commentText.Text);
		}
	}
}

using System;
using System.Xml;
using System.Collections;

namespace Stetic.Wrapper {

	public class ButtonBox : Box {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			foreach (Gtk.Widget child in buttonbox.Children) {
				if (child is Placeholder)
					ReplaceChild (child, NewButton ());
			}
		}

		Gtk.Button NewButton ()
		{
			Gtk.Button button = (Gtk.Button)Registry.NewInstance ("Gtk.Button", proj);
			if (InternalChildProperty != null && InternalChildProperty.Name == "ActionArea")
				((Button)Widget.Lookup (button)).IsDialogButton = true;
			return button;
		}

		protected Gtk.ButtonBox buttonbox {
			get {
				return (Gtk.ButtonBox)Wrapped;
			}
		}

		protected override bool AllowPlaceholders {
			get {
				return false;
			}
		}
		internal new void InsertBefore (Gtk.Widget context)
		{
			int position;
			bool secondary;

			if (context == buttonbox) {
				position = 0;
				secondary = false;
			} else {
				Gtk.ButtonBox.ButtonBoxChild bbc = (Gtk.ButtonBox.ButtonBoxChild)ContextChildProps (context);
				position = bbc.Position;
				secondary = bbc.Secondary;
			}

			Gtk.Button button = NewButton ();
			buttonbox.PackStart (button, false, false, 0);
			buttonbox.ReorderChild (button, position);
			buttonbox.SetChildSecondary (button, secondary);
			EmitContentsChanged ();
		}

		internal new void InsertAfter (Gtk.Widget context)
		{
			int position;
			bool secondary;

			if (context == buttonbox) {
				position = buttonbox.Children.Length - 1;
				secondary = false;
			} else {
				Gtk.ButtonBox.ButtonBoxChild bbc = (Gtk.ButtonBox.ButtonBoxChild)ContextChildProps (context);
				position = bbc.Position;
				secondary = bbc.Secondary;
			}

			Gtk.Button button = NewButton ();
			buttonbox.PackStart (button, false, false, 0);
			buttonbox.ReorderChild (button, position + 1);
			buttonbox.SetChildSecondary (button, secondary);
			EmitContentsChanged ();
		}

		public int Size {
			get {
				return buttonbox.Children.Length;
			}
			set {
				Gtk.Widget[] children = buttonbox.Children;
				int cursize = children.Length;

				while (cursize > value)
					buttonbox.Remove (children[--cursize]);
				while (cursize < value) {
					buttonbox.PackStart (NewButton (), false, false, 0);
					cursize++;
				}
			}
		}
		
		protected override void ReadChildren (XmlElement elem, FileFormat format)
		{
			// Reset the button count
			Size = 0;
			base.ReadChildren (elem, format);
		}

		public class ButtonBoxChild : Box.BoxChild {

			public bool InDialog {
				get {
					if (ParentWrapper == null)
						return false;
					return ParentWrapper.InternalChildProperty != null && ParentWrapper.InternalChildProperty.Name == "ActionArea";
				}
			}
		}
	}
}

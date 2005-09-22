using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class ButtonBox : Box {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			FillButtons ();
		}

		void FillButtons ()
		{
			foreach (Gtk.Widget child in buttonbox.Children) {
				if (child is Placeholder)
					ReplaceChild (child, (Gtk.Button)Registry.NewInstance (typeof (Gtk.Button), proj));
			}
		}

		protected Gtk.ButtonBox buttonbox {
			get {
				return (Gtk.ButtonBox)Wrapped;
			}
		}

		internal new void InsertBefore (Gtk.Widget context)
		{
			base.InsertBefore (context);
			FillButtons ();
		}

		internal new void InsertAfter (Gtk.Widget context)
		{
			base.InsertAfter (context);
			FillButtons ();
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
					buttonbox.PackStart ((Gtk.Button)Registry.NewInstance (typeof (Gtk.Button), proj), false, false, 0);
					cursize++;
				}
			}
		}
	}
}

using Gtk;
using GLib;
using System;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class Text : Gtk.ScrolledWindow {

		Gtk.TextView textview;

		public Text (string value)
		{
			ShadowType = Gtk.ShadowType.In;
			SetPolicy (Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);

			textview = new Gtk.TextView ();
			textview.WrapMode = Gtk.WrapMode.Word;
			textview.Show ();
			Add (textview);

			Pango.Context ctx = textview.PangoContext;
			Pango.FontMetrics metrics = ctx.GetMetrics (textview.Style.FontDescription,
								    ctx.Language);

			int lineHeight = (metrics.Ascent + metrics.Descent) / (int)Pango.Scale.PangoScale;

			// FIXME: make # of lines adjustable
			SetSizeRequest (-1, lineHeight * 6);

			if (value != null)
				textview.Buffer.Text = value;

			textview.Buffer.Changed += Buffer_Changed;
		}

		public void Buffer_Changed (object obj, EventArgs args)
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		public string Value {
			get {
				return textview.Buffer.Text;
			}
			set {
				textview.Buffer.Text = value;
			}
		}

		public event EventHandler Changed;
	}
}

using System;
using System.Reflection;

namespace Stetic.Editor {

	[PropertyEditor ("Value", "Changed")]
	public class Text : Gtk.ScrolledWindow {

		Gtk.TextView textview;

		public Text (PropertyInfo info)
		{
			ShadowType = Gtk.ShadowType.In;
			SetPolicy (Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);

			textview = new Gtk.TextView ();
			textview.WrapMode = Gtk.WrapMode.Word;
			textview.Show ();
			Add (textview);

			int nlines = 6;
			foreach (EditorAttribute eattr in info.GetCustomAttributes (typeof (Stetic.EditorAttribute), false)) {
				if (eattr.EditorSize != -1)
					nlines = eattr.EditorSize;
			}

			Pango.Context ctx = textview.PangoContext;
			Pango.FontMetrics metrics = ctx.GetMetrics (textview.Style.FontDescription,
								    ctx.Language);
			int lineHeight = (metrics.Ascent + metrics.Descent) / (int)Pango.Scale.PangoScale;
			SetSizeRequest (-1, lineHeight * nlines);

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

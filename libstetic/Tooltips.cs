using System;
using System.Collections;
using System.Xml;

namespace Stetic {

	public class Tooltips : Hashtable, IDisposable {

		Gtk.Tooltips tips;

		public Tooltips ()
		{
			Clear ();
		}

		~Tooltips ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			tips.Destroy ();
			GC.SuppressFinalize (this);
		}

		void SetTip (object key, object value)
		{
			Gtk.Widget widget = key as Gtk.Widget;
			string tip = value as string;

			if (widget == null)
				return;
			if (tip == "")
				tip = null;
			tips.SetTip (widget, tip, tip);
		}

		public override void Add (object key, object value)
		{
			base.Add (key, value);
			SetTip (key, value);
		}

		public override void Clear ()
		{
			if (tips != null)
				tips.Destroy ();
			tips = new Gtk.Tooltips ();
			tips.Enable ();
			base.Clear ();
		}

		public override void Remove (object key)
		{
			base.Remove (key);
			SetTip (key, null);
		}

		public override object this[object key] {
			get {
				return base[key];
			}
			set {
				base[key] = value;
				SetTip (key, value);
			}
		}

		public string this[Gtk.Widget key] {
			get {
				return base[key] as string;
			}
			set {
				base[key] = value;
				SetTip (key, value);
			}
		}
	}
}

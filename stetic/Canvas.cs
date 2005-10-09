using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public class Canvas : Gtk.Layout {

		public Canvas () : base (null, null)
		{
			ThemeChanged += CanvasThemeChanged;
		}

		ArrayList previews = new ArrayList ();

		void CanvasThemeChanged ()
		{
			foreach (Metacity.Preview prev in previews)
				prev.Theme = Theme;
		}

		static Metacity.Theme theme;
		static Metacity.Theme Theme {
			get {
				if (theme == null) {
					GConf.Client client = new GConf.Client ();
					client.AddNotify ("/apps/metacity/general", GConfNotify);
					string themeName = (string)client.Get ("/apps/metacity/general/theme");
					theme = Metacity.Theme.Load (themeName);
				}
				return theme;
			}
		}

		static void GConfNotify (object obj, GConf.NotifyEventArgs args)
		{
			if (args.Key == "/apps/metacity/general/theme") {
				theme = Metacity.Theme.Load ((string)args.Value);
				if (ThemeChanged != null)
					ThemeChanged ();
			}
		}

		delegate void ThemeChangedHandler ();
		static event ThemeChangedHandler ThemeChanged;

		protected override void OnRealized ()
		{
			base.OnRealized ();
			ModifyBg (Gtk.StateType.Normal, Style.Base (Gtk.StateType.Normal));
		}

		public void Add (Window win)
		{
			Metacity.Preview prev = new Metacity.Preview ();
			prev.Title = ((Gtk.Window)win).Title;
			prev.Theme = Theme;
			if (win is Gtk.Dialog)
				prev.FrameType = Metacity.FrameType.Dialog;
			else
				prev.FrameType = Metacity.FrameType.Normal;
			prev.FrameFlags =
				Metacity.FrameFlags.AllowsDelete |
				Metacity.FrameFlags.AllowsVerticalResize |
				Metacity.FrameFlags.AllowsHorizontalResize |
				Metacity.FrameFlags.AllowsMove |
				Metacity.FrameFlags.AllowsShade;

			previews.Add (prev);

			Put (win, 20, 20);
		}
	}
}

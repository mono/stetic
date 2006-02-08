using System;
using System.Runtime.InteropServices;

namespace Stetic.Editor {

	[PropertyEditor ("Accel", "AccelChanged")]
	public class Accelerator : Gtk.Entry, IPropertyEditor {

		uint keyval;
		Gdk.ModifierType mask;
		bool editing;

		const Gdk.ModifierType AcceleratorModifierMask = ~(
			Gdk.ModifierType.Button1Mask |
			Gdk.ModifierType.Button2Mask |
			Gdk.ModifierType.Button3Mask |
			Gdk.ModifierType.Button4Mask |
			Gdk.ModifierType.Button5Mask);

		public Accelerator ()
		{
			IsEditable = false;
		}

		public void Initialize (PropertyDescriptor descriptor)
		{
			if (descriptor.PropertyType != typeof(string))
				throw new ApplicationException ("Accelerator editor does not support editing values of type " + descriptor.PropertyType);
		}
		
		public void AttachObject (object obj)
		{
			Value = null;
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			if (editing)
				Ungrab (evt.Time);
			else
				Grab (evt.Window, evt.Time);
			return true;
		}

		void Ungrab (uint time)
		{
			if (!editing)
				return;
			editing = false;

			Gdk.Keyboard.Ungrab (time);
			Gdk.Pointer.Ungrab (time);
			Text = (string) Value;
		}

		void Grab (Gdk.Window window, uint time)
		{
			if (editing)
				return;

			if (Gdk.Keyboard.Grab (window, false, time) != Gdk.GrabStatus.Success)
				return;
			if (Gdk.Pointer.Grab (window, false, Gdk.EventMask.ButtonPressMask,
					      null, null, time) != Gdk.GrabStatus.Success) {
				Gdk.Keyboard.Ungrab (time);
				return;
			}
			GrabFocus ();

			editing = true;
			Text = "New Accelerator...";
		}

		[DllImport ("libsteticglue")]
		static extern bool stetic_keycode_is_modifier (uint keycode);

		protected override bool OnKeyPressEvent (Gdk.EventKey evt)
		{
			if (!editing || stetic_keycode_is_modifier (evt.HardwareKeycode))
				return base.OnKeyPressEvent (evt);

			uint keyval;
			int effectiveGroup, level;
			Gdk.ModifierType consumedMods, mask;

			// We know this will succeed, since we're already here...
			Gdk.Keymap.Default.TranslateKeyboardState (evt.HardwareKeycode, evt.State, evt.Group, out keyval, out effectiveGroup, out level, out consumedMods);
			mask = evt.State & AcceleratorModifierMask & ~consumedMods;

			if (evt.Key != Gdk.Key.Escape || mask != 0) {
				this.keyval = keyval;
				this.mask = mask;
			}

			Ungrab (evt.Time);
			EmitAccelChanged ();
			return true;
		}

		public new string Text {
			set {
				if (value == null)
					base.Text = "";
				else
					base.Text = value;
			}
		}

		public object Value {
			get {
				if (keyval != 0)
					return Gtk.Accelerator.Name (keyval, mask);
				else
					return null;
			}
			set {
				string s = value as string;
				if (s == null) {
					keyval = 0;
					mask = 0;
				} else
					Gtk.Accelerator.Parse (s, out keyval, out mask);
				Text = (string) Value;
				EmitAccelChanged ();
			}
		}

		public event EventHandler ValueChanged;

		void EmitAccelChanged ()
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}
	}
}

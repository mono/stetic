using System;
using System.Runtime.InteropServices;

namespace Stetic.Editor {

	[PropertyEditor ("Accel", "AccelChanged")]
	public class Accelerator : Gtk.HBox, IPropertyEditor {

		uint keyval;
		Gdk.ModifierType mask;
		bool editing;
		
		Gtk.Button clearButton;
		Gtk.Entry entry;

		const Gdk.ModifierType AcceleratorModifierMask = ~(
			Gdk.ModifierType.Button1Mask |
			Gdk.ModifierType.Button2Mask |
			Gdk.ModifierType.Button3Mask |
			Gdk.ModifierType.Button4Mask |
			Gdk.ModifierType.Button5Mask);

		public Accelerator ()
		{
			entry = new Gtk.Entry ();
			clearButton = new Gtk.Button (new Gtk.Image (Gtk.Stock.Clear, Gtk.IconSize.Menu));
			PackStart (entry, true, true, 0);
			PackStart (clearButton, false, false, 3);
			clearButton.Clicked += delegate (object s, EventArgs args) {
				Value = null;
			};
			entry.IsEditable = false;
			entry.ButtonPressEvent += OnButtonPressEvent;
			entry.KeyPressEvent += OnKeyPressEvent;
			ShowAll ();
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
		
		[GLib.ConnectBefore]
		void OnButtonPressEvent (object s, Gtk.ButtonPressEventArgs args)
		{
			if (editing)
				Ungrab (args.Event.Time);
			else
				Grab (args.Event.Window, args.Event.Time);
			args.RetVal = true;
		}

		void Ungrab (uint time)
		{
			if (!editing)
				return;
			editing = false;

			Gdk.Keyboard.Ungrab (time);
			Gdk.Pointer.Ungrab (time);
			if (Value != null)
				entry.Text = (string) Value;
			else
				entry.Text = "";
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
			entry.GrabFocus ();

			editing = true;
			entry.Text = "Press a key...";
		}

		[GLib.ConnectBefore]
		void OnKeyPressEvent (object s, Gtk.KeyPressEventArgs args)
		{
			Gdk.EventKey evt = args.Event;
			
			if (!editing || !Gtk.Accelerator.Valid (evt.KeyValue, evt.State))
				return;
			
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
			
			clearButton.Sensitive = true;

			Ungrab (evt.Time);
			EmitAccelChanged ();
			args.RetVal = true;
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
					clearButton.Sensitive = false;
				} else {
					Gtk.Accelerator.Parse (s, out keyval, out mask);
					clearButton.Sensitive = true;
				}
				if (Value != null)
					entry.Text = (string) Value;
				else
					entry.Text = "";
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

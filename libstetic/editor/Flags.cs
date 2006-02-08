using Gtk;
using System;
using System.Collections;

namespace Stetic.Editor {

	public class Flags : Gtk.Frame, IPropertyEditor {

		EnumDescriptor enm;
		Hashtable flags;
		Gtk.Tooltips tips;

		public Flags ()
		{
		}
		
		public void Initialize (PropertyDescriptor prop)
		{
			if (!prop.PropertyType.IsEnum)
				throw new ApplicationException ("Flags editor does not support editing values of type " + prop.PropertyType);

			Gtk.VBox vbox = new Gtk.VBox (true, 3);
			Add (vbox);

			tips = new Gtk.Tooltips ();
			flags = new Hashtable ();

			enm = Registry.LookupEnum (prop.PropertyType);
			foreach (Enum value in enm.Values) {
				EnumValue eval = enm[value];
				if (eval.Label == "")
					continue;

				Gtk.CheckButton check = new Gtk.CheckButton (eval.Label);
				tips.SetTip (check, eval.Description, eval.Description);
				uint uintVal = (uint)(int)eval.Value;
				flags[check] = uintVal;
				flags[uintVal] = check;
				
				check.Toggled += FlagToggled;
				vbox.PackStart (check, false, false, 0);
			}

			vbox.ShowAll ();
		}

		public void AttachObject (object ob)
		{
		}

		public override void Dispose ()
		{
			tips.Destroy ();
			base.Dispose ();
		}

		public object Value {
			get {
				return Enum.ToObject (enm.EnumType, UIntValue);
			}
			set {
				uint newVal = (uint)(int)(Enum)value;
				for (uint i = 1; i <= uintValue || i <= newVal; i = i << 1) {
					if ((uintValue & i) != (newVal & i)) {
						Gtk.CheckButton check = (Gtk.CheckButton)flags[i];
						if (check != null)
							check.Active = !check.Active;
					}
				}
			}
		}

		public event EventHandler ValueChanged;

		uint uintValue;
		uint UIntValue {
			get {
				return uintValue;
			}
			set {
				if (uintValue != value) {
					uintValue = value;
					if (ValueChanged != null)
						ValueChanged (this, EventArgs.Empty);
				}
			}
		}

		void FlagToggled (object o, EventArgs args)
		{
			Gtk.CheckButton check = (Gtk.CheckButton)o;
			uint val = (uint)flags[o];

			if (check.Active)
				UIntValue |= val;
			else
				UIntValue &= ~val;
		}
	}
}

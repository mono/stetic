using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	static class PropertyEditors {
		static Hashtable editors;

		static PropertyEditors ()
		{
			editors = new Hashtable ();

			foreach (MethodInfo info in typeof (PropertyEditors).GetMethods (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				if (info.ReturnType != typeof (Widget))
					continue;

				ParameterInfo[] param = info.GetParameters ();
				if (param.Length != 3 ||
				    !param[0].ParameterType.IsSubclassOf (typeof (ParamSpec)) ||
				    param[0].ParameterType == typeof (ParamSpec) ||
				    param[1].ParameterType != typeof (PropertyInfo) ||
				    param[2].ParameterType != typeof (object))
					continue;

				editors[param[0].ParameterType] = info;
			}
		}

		static public Widget MakeEditor (ParamSpec pspec, PropertyInfo info, object obj)
		{
			MethodInfo editor = editors[pspec.GetType ()] as MethodInfo;
			if (editor != null) {
				object[] args = { pspec, info, obj };
				return (Widget)editor.Invoke (null, args);
			} else
				return new Label ("(unknown type)");
		}

		// The editors...

		static Widget Boolean (ParamSpecBoolean pspec, PropertyInfo info, object obj)
		{
			CheckButton cb = new Gtk.CheckButton ("");

			if (info.CanRead)
				cb.Active = (bool)info.GetValue (obj, null);
			else
				cb.Active = pspec.Default;

			if (info.CanWrite) {
				cb.Toggled += delegate (object o, EventArgs args) {
					info.SetValue (obj, cb.Active, null);
				};
			} else
				cb.Sensitive = false;

			return cb;
		}

		static Widget Char (ParamSpecChar pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (sbyte)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget UChar (ParamSpecUChar pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (byte)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget Int (ParamSpecInt pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (int)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget UInt (ParamSpecUInt pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (uint)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget Long (ParamSpecLong pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (int)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget ULong (ParamSpecULong pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (uint)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget Int64 (ParamSpecInt64 pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (long)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget UInt64 (ParamSpecUInt64 pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum, 1.0);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (ulong)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget Float (ParamSpecFloat pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum,
							    pspec.Epsilon > 0.01 ? pspec.Epsilon : 0.01);

			if (info.CanRead)
				sb.Value = Convert.ToDouble (info.GetValue (obj, null));
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (float)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget Double (ParamSpecDouble pspec, PropertyInfo info, object obj)
		{
			SpinButton sb = new Gtk.SpinButton (pspec.Minimum, pspec.Maximum,
							    pspec.Epsilon > 0.01 ? pspec.Epsilon : 0.01);

			if (info.CanRead)
				sb.Value = (double)info.GetValue (obj, null);
			else
				sb.Value = (double)pspec.Default;

			if (info.CanWrite) {
				sb.ValueChanged += delegate (object o, EventArgs args) {
					info.SetValue (obj, (double)sb.Value, null);
				};
			} else
				sb.Sensitive = false;

			return sb;
		}

		static Widget String (ParamSpecString pspec, PropertyInfo info, object obj)
		{
			Entry entry = new Gtk.Entry ();

			if (info.CanRead)
				entry.Text = (string)info.GetValue (obj, null);
			else if (pspec.Default != null)
				entry.Text = pspec.Default;

			if (info.CanWrite) {
				entry.Changed += delegate (object o, EventArgs args) {
					info.SetValue (obj, entry.Text, null);
				};
			} else
				entry.Sensitive = false;

			return entry;
		}

		// Can't currently use an anon delegate here because of
		// bug 69614
		private class EnumHelper {
			PropertyInfo info;
			object obj, value;

			public EnumHelper (PropertyInfo info, object obj, int value)
			{
				this.info = info;
				this.obj = obj;
				this.value = System.Enum.ToObject (info.PropertyType, value);
			}

			public void Activated (object item, EventArgs args)
			{
				info.SetValue (obj, value, null);
			}
		}

		static Widget Enum (ParamSpecEnum pspec, PropertyInfo info, object obj)
		{
			OptionMenu om;
			Menu menu;
			int defaultValue, defaultItem;

			om = new OptionMenu ();
			menu = new Menu ();
			om.Menu = menu;

			if (info.CanRead)
				defaultValue = (int)info.GetValue (obj, null);
			else
				defaultValue = pspec.Default;

			for (int val = pspec.Minimum, nItems = 0; val <= pspec.Maximum; val++) {
				string name = pspec.ValueName (val);
				if (name == null)
					continue;

				MenuItem item = new MenuItem (name + " (" + val.ToString() + ")");
				if (info.CanWrite) {
					EnumHelper eh = new EnumHelper (info, obj, val);
					item.Activated += eh.Activated;
				} else
					item.Sensitive = false;
				menu.Add (item);

				if (val == defaultValue)
					om.SetHistory ((uint) nItems);
				nItems++;
			}

			return om;
		}

		// Can't currently use an anon delegate here because of
		// bug 69614
		private class FlagsHelper {
			PropertyInfo info;
			object obj;
			uint value;

			public FlagsHelper (PropertyInfo info, object obj, uint value)
			{
				this.info = info;
				this.obj = obj;
				this.value = value;
			}

			public void Toggled (object item, EventArgs args)
			{
				uint cur = Convert.ToUInt32 (info.GetValue (obj, null));
				if (((CheckMenuItem)item).Active)
					cur |= value;
				else
					cur &= ~value;
				info.SetValue (obj, System.Enum.ToObject (info.PropertyType, cur), null);
			}
		}

		static Widget Flags (ParamSpecFlags pspec, PropertyInfo info, object obj)
		{
			OptionMenu om;
			Menu menu;
			MenuItem item;
			uint defaultValue, mask, bit;

			om = new OptionMenu ();
			menu = new Menu ();
			om.Menu = menu;

			if (info.CanRead)
				defaultValue = Convert.ToUInt32 (info.GetValue (obj, null));
			else
				defaultValue = pspec.Default;

			item = new MenuItem (System.String.Format ("{0:x}...", defaultValue));
			item.Sensitive = false;
			menu.Add (item);

			mask = pspec.Mask;
			bit = 1;
			while (mask != 0) {
				while ((mask & 0x1) == 0) {
					mask = mask >> 1;
					bit = bit << 1;
				}
				mask &= ~1U;

				string name = pspec.ValueName (bit);
				if (name == null)
					continue;

				CheckMenuItem citem = new CheckMenuItem (System.String.Format ("{0} ({1:x})", name, bit));
				if (info.CanWrite) {
					FlagsHelper fh = new FlagsHelper (info, obj, bit);
					citem.Toggled += fh.Toggled;
				} else
					citem.Sensitive = false;
				menu.Add (citem);

				if ((defaultValue & bit) == bit)
					citem.Active = true;
			}

			om.SetHistory (0);
			return om;
		}

		static Widget Pointer (ParamSpecPointer pspec, PropertyInfo info, object obj)
		{
			return new Gtk.Label ("(pointer)");
		}

		static Widget Boxed (ParamSpecBoxed pspec, PropertyInfo info, object obj)
		{
			return new Gtk.Label ("(boxed)");
		}

		static Widget Param (ParamSpecParam pspec, PropertyInfo info, object obj)
		{
			return new Gtk.Label ("(param)");
		}

		static Widget Object (ParamSpecObject pspec, PropertyInfo info, object obj)
		{
			return new Gtk.Label ("(object)");
		}
	}
}

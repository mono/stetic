// GLib.ParamSpec.cs - ParamSpec implementation
//
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


using GLib;
using Gtk;

namespace Stetic {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public class ParamSpec : IWrapper, IDisposable {
		IntPtr _obj;

		protected ParamSpec (IntPtr raw)
		{
			Raw = raw;
		}

		~ParamSpec ()
		{
			Dispose ();
		}

		public void Dispose () 
		{
			Raw = IntPtr.Zero;
			GC.SuppressFinalize (this);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_param_spec_ref (IntPtr obj);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_param_spec_unref (IntPtr obj);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_param_spec_sink (IntPtr obj);

		protected IntPtr Raw {
			get {
				return _obj;
			}
			set {
				if (_obj != IntPtr.Zero)
					g_param_spec_unref (_obj);
				_obj = value;
				if (_obj != IntPtr.Zero) {
					g_param_spec_ref (_obj);
					g_param_spec_sink (_obj);
				}
			}
		}

		public IntPtr Handle {
			get {
				return _obj;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_param_spec_get_name (IntPtr obj);

		public string Name {
			get {
				return Marshal.PtrToStringAnsi (g_param_spec_get_name (_obj));
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_param_spec_get_nick (IntPtr obj);

		public string Nick {
			get {
				return Marshal.PtrToStringAnsi (g_param_spec_get_nick (_obj));
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_param_spec_get_blurb (IntPtr obj);

		public string Blurb {
			get {
				return Marshal.PtrToStringAnsi (g_param_spec_get_blurb (_obj));
			}
		}

		public virtual object Minimum {
			get {
				return null;
			}
		}

		public virtual object Maximum {
			get {
				return null;
			}
		}

		public virtual object Default {
			get {
				return null;
			}
		}

		[DllImport("libsteticglue")]
		static extern ParamFlags stetic_param_spec_get_flags (IntPtr obj);

		public ParamFlags Flags {
			get {
				return stetic_param_spec_get_flags (_obj);
			}
		}

		[DllImport("libsteticglue")]
		static extern GType stetic_param_spec_get_value_type (IntPtr obj);

		public GType ValueType {
			get {
				return stetic_param_spec_get_value_type (_obj);
			}
		}

		[DllImport("libsteticglue")]
		static extern GType stetic_param_spec_get_owner_type (IntPtr obj);

		public GType OwnerType {
			get {
				return stetic_param_spec_get_owner_type (_obj);
			}
		}

		static Hashtable typemap;

		[DllImport("libglibsharpglue-2.0.dll")]
		static extern int gtksharp_get_type_id (IntPtr raw);

		static public ParamSpec Wrap (IntPtr raw)
		{
			Type type = typemap[gtksharp_get_type_id (raw)] as Type;

			if (type != null) {
				object[] args = { raw };
				return (ParamSpec) System.Activator.CreateInstance (type, args);
			} else
				return new ParamSpec (raw);
		}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_for_property (IntPtr obj, string property_name);

		static public ParamSpec LookupObjectProperty (GLib.Object obj, string property)
		{
			IntPtr raw_ret = stetic_param_spec_for_property (obj.Handle, property);
			if (raw_ret == IntPtr.Zero)
				return null;
			else
				return ParamSpec.Wrap (raw_ret);
		}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_for_child_property (IntPtr obj, string property_name);

		static public ParamSpec LookupChildProperty (GLib.Object obj, string property)
		{
			IntPtr raw_ret = stetic_param_spec_for_child_property (obj.Handle, property);
			if (raw_ret == IntPtr.Zero)
				return null;
			else
				return ParamSpec.Wrap (raw_ret);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern int g_type_from_name (string name);

		static ParamSpec ()
		{
			typemap = new Hashtable ();

			typemap[g_type_from_name ("GParamChar")] = typeof (ParamSpecChar);
			typemap[g_type_from_name ("GParamUChar")] = typeof (ParamSpecUChar);
			typemap[g_type_from_name ("GParamInt")] = typeof (ParamSpecInt);
			typemap[g_type_from_name ("GParamUInt")] = typeof (ParamSpecUInt);
			typemap[g_type_from_name ("GParamLong")] = typeof (ParamSpecLong);
			typemap[g_type_from_name ("GParamULong")] = typeof (ParamSpecULong);
			typemap[g_type_from_name ("GParamInt64")] = typeof (ParamSpecInt64);
			typemap[g_type_from_name ("GParamUInt64")] = typeof (ParamSpecUInt64);
			typemap[g_type_from_name ("GParamFloat")] = typeof (ParamSpecFloat);
			typemap[g_type_from_name ("GParamDouble")] = typeof (ParamSpecDouble);
			typemap[g_type_from_name ("GParamBoolean")] = typeof (ParamSpecBoolean);
			typemap[g_type_from_name ("GParamEnum")] = typeof (ParamSpecEnum);
			typemap[g_type_from_name ("GParamFlags")] = typeof (ParamSpecFlags);
			typemap[g_type_from_name ("GParamUnichar")] = typeof (ParamSpecUnichar);
			typemap[g_type_from_name ("GParamString")] = typeof (ParamSpecString);
			typemap[g_type_from_name ("GParamParam")] = typeof (ParamSpecParam);
			typemap[g_type_from_name ("GParamBoxed")] = typeof (ParamSpecBoxed);
			typemap[g_type_from_name ("GParamPointer")] = typeof (ParamSpecPointer);
			typemap[g_type_from_name ("GParamValueArray")] = typeof (ParamSpecValueArray);
			typemap[g_type_from_name ("GParamObject")] = typeof (ParamSpecObject);
			typemap[g_type_from_name ("GParamOverride")] = typeof (ParamSpecOverride);
		}
	}

	public class ParamSpecChar : ParamSpec {
		public ParamSpecChar (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern sbyte stetic_param_spec_char_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_char_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern sbyte stetic_param_spec_char_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_char_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern sbyte stetic_param_spec_char_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_char_get_default (Raw);
			}
		}
	}

	public class ParamSpecUChar : ParamSpec {
		public ParamSpecUChar (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern byte stetic_param_spec_uchar_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_uchar_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern byte stetic_param_spec_uchar_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_uchar_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern byte stetic_param_spec_uchar_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_uchar_get_default (Raw);
			}
		}
	}

	public class ParamSpecBoolean : ParamSpec {
		public ParamSpecBoolean (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern bool stetic_param_spec_boolean_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_boolean_get_default (Raw);
			}
		}
	}

	public class ParamSpecInt : ParamSpec {
		public ParamSpecInt (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_int_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_int_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_int_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_int_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_int_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_int_get_default (Raw);
			}
		}
	}

	public class ParamSpecUInt : ParamSpec {
		public ParamSpecUInt (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_uint_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_uint_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_uint_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_uint_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_uint_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_uint_get_default (Raw);
			}
		}
	}

	public class ParamSpecLong : ParamSpec {
		public ParamSpecLong (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_long_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_long_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_long_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_long_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_long_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_long_get_default (Raw);
			}
		}
	}

	public class ParamSpecULong : ParamSpec {
		public ParamSpecULong (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_ulong_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_ulong_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_ulong_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_ulong_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_ulong_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_ulong_get_default (Raw);
			}
		}
	}

	public class ParamSpecInt64 : ParamSpec {
		public ParamSpecInt64 (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern long stetic_param_spec_int64_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_int64_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern long stetic_param_spec_int64_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_int64_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern long stetic_param_spec_int64_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_int64_get_default (Raw);
			}
		}
	}

	public class ParamSpecUInt64 : ParamSpec {
		public ParamSpecUInt64 (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern ulong stetic_param_spec_uint64_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_uint64_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern ulong stetic_param_spec_uint64_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_uint64_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern ulong stetic_param_spec_uint64_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_uint64_get_default (Raw);
			}
		}
	}

	public class ParamSpecUnichar : ParamSpec {
		public ParamSpecUnichar (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_unichar_get_default (IntPtr obj);

		public override object Default {
			get {
				return Marshaller.GUnicharToChar (stetic_param_spec_unichar_get_default (Raw));
			}
		}
	}

	public class ParamSpecEnum : ParamSpec {
		public ParamSpecEnum (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_enum_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_enum_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_enum_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_enum_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern int stetic_param_spec_enum_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_enum_get_default (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_enum_get_value_name (IntPtr obj, int value);

		public string ValueName (int value) {
			IntPtr raw_ret = stetic_param_spec_enum_get_value_name (Raw, value);
			if (raw_ret == IntPtr.Zero)
				return null;
			else
				return Marshal.PtrToStringAnsi (raw_ret);
		}
	}

	public class ParamSpecFlags : ParamSpec {
		public ParamSpecFlags (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_flags_get_mask (IntPtr obj);

		public uint Mask {
			get {
				return stetic_param_spec_flags_get_mask (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_flags_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_flags_get_default (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_flags_get_value_name (IntPtr obj, uint value);

		public string ValueName (uint value) {
			IntPtr raw_ret = stetic_param_spec_flags_get_value_name (Raw, value);
			if (raw_ret == IntPtr.Zero)
				return null;
			else
				return Marshal.PtrToStringAnsi (raw_ret);
		}
	}

	public class ParamSpecFloat : ParamSpec {
		public ParamSpecFloat (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern float stetic_param_spec_float_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_float_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern float stetic_param_spec_float_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_float_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern float stetic_param_spec_float_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_float_get_default (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern float stetic_param_spec_float_get_epsilon (IntPtr obj);

		public float Epsilon {
			get {
				return stetic_param_spec_float_get_epsilon (Raw);
			}
		}
	}

	public class ParamSpecDouble : ParamSpec {
		public ParamSpecDouble (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern double stetic_param_spec_double_get_minimum (IntPtr obj);

		public override object Minimum {
			get {
				return stetic_param_spec_double_get_minimum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern double stetic_param_spec_double_get_maximum (IntPtr obj);

		public override object Maximum {
			get {
				return stetic_param_spec_double_get_maximum (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern double stetic_param_spec_double_get_default (IntPtr obj);

		public override object Default {
			get {
				return stetic_param_spec_double_get_default (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern double stetic_param_spec_double_get_epsilon (IntPtr obj);

		public double Epsilon {
			get {
				return stetic_param_spec_double_get_epsilon (Raw);
			}
		}
	}

	public class ParamSpecString : ParamSpec {
		public ParamSpecString (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_string_get_default (IntPtr obj);

		public override object Default {
			get {
				return Marshal.PtrToStringAnsi (stetic_param_spec_string_get_default (Raw));
			}
		}
	}

	public class ParamSpecParam : ParamSpec {
		public ParamSpecParam (IntPtr raw) : base (raw) {}
	}

	public class ParamSpecBoxed : ParamSpec {
		public ParamSpecBoxed (IntPtr raw) : base (raw) {}
	}

	public class ParamSpecPointer : ParamSpec {
		public ParamSpecPointer (IntPtr raw) : base (raw) {}
	}

	public class ParamSpecValueArray : ParamSpec {
		public ParamSpecValueArray (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern uint stetic_param_spec_value_array_get_fixed_n_elements (IntPtr obj);

		public uint FixedNElements {
			get {
				return stetic_param_spec_value_array_get_fixed_n_elements (Raw);
			}
		}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_value_array_get_element_spec (IntPtr obj);

		public ParamSpec ElementSpec {
			get {
				IntPtr ret_raw = stetic_param_spec_value_array_get_element_spec (Raw);
				return ParamSpec.Wrap (ret_raw);
			}
		}
	}

	public class ParamSpecObject : ParamSpec {
		public ParamSpecObject (IntPtr raw) : base (raw) {}
	}

	public class ParamSpecOverride : ParamSpec {
		public ParamSpecOverride (IntPtr raw) : base (raw) {}

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_param_spec_override_get_overridden (IntPtr obj);

		public ParamSpec Overridden {
			get {
				IntPtr ret_raw = stetic_param_spec_override_get_overridden (Raw);
				return ParamSpec.Wrap (ret_raw);
			}
		}
	}

}

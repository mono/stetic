using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {

	public delegate void NotifyDelegate (ParamSpec pspec);
	delegate void NotifyDelegateInternal (IntPtr obj_raw, IntPtr pspec_raw, IntPtr data);

	static class Notify {
		static Hashtable callbacks;
		static int nextId;

		static Notify ()
		{
			callbacks = new Hashtable ();
		}

		static void NotifyWrapper (IntPtr obj_raw, IntPtr pspec_raw, IntPtr id)
		{
			ParamSpec pspec = ParamSpec.Wrap (pspec_raw);
			NotifyDelegate nd = callbacks[id] as NotifyDelegate;
			if (nd != null)
				nd (pspec);
		}

		[DllImport("steticglue")]
		static extern IntPtr stetic_notify_connect (IntPtr raw, NotifyDelegateInternal callback, IntPtr data);

		public static IntPtr Add (GLib.Object obj, NotifyDelegate callback)
		{
			IntPtr id = (IntPtr)nextId++;
			callbacks[id] = callback;
			return stetic_notify_connect (obj.Handle, new NotifyDelegateInternal (NotifyWrapper), id);
		}

		[DllImport("steticglue")]
		static extern void stetic_notify_disconnect (IntPtr raw, IntPtr id);

		public static void Remove (GLib.Object obj, IntPtr id)
		{
			stetic_notify_disconnect (obj.Handle, id);
		}
	}
}

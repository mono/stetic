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

			editors[typeof (bool)] = typeof (Stetic.Editor.Boolean);
			editors[typeof (byte)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (sbyte)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (short)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (ushort)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (int)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (uint)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (long)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (ulong)] = typeof (Stetic.Editor.IntRange);
			editors[typeof (float)] = typeof (Stetic.Editor.FloatRange);
			editors[typeof (double)] = typeof (Stetic.Editor.FloatRange);
			editors[typeof (char)] = typeof (Stetic.Editor.Char);
			editors[typeof (string)] = typeof (Stetic.Editor.String);
		}

		static public Widget MakeEditor (PropertyDescriptor prop, ParamSpec pspec, object obj)
		{
			Type editorType;
			object min, max, initial;

			if (pspec != null) {
				min = pspec.Minimum;
				max = pspec.Maximum;
				initial = pspec.Default;
			} else {
				min = max = null;
				initial = prop.Default;
			}

			if (prop.CanRead)
				initial = prop.GetValue (obj);

			editorType = prop.EditorType;
			if (editorType == null) {
				if (prop.PropertyType.IsEnum) {
					editorType = typeof (Stetic.Editor.Enumeration);
					min = max = null;
				} else if (prop.PropertyType == typeof (int) && ((int)min == -1)) {
					editorType = typeof (Stetic.Editor.OptIntRange);
					min = 0;
				} else {
					editorType = editors[prop.PropertyType] as Type;
					if (editorType == null)
						return NoEditor (prop);
				}
			}

			if (min != null && max != null)
				return RangeEditor (editorType, min, max, initial, obj, prop);
			else
				return BasicEditor (editorType, initial, obj, prop);
		}

		private class BasicEditorHelper {
			PropertyDescriptor prop;
			PropertyInfo vprop;
			object editor, obj;

			public BasicEditorHelper (PropertyDescriptor prop, object obj,
						  PropertyInfo vprop, object editor)
			{
				this.prop = prop;
				this.obj = obj;
				this.vprop = vprop;
				this.editor = editor;
			}

			public void ValueChanged (object o, EventArgs args)
			{
				prop.SetValue (obj, vprop.GetValue (editor, null));
			}
		}

		static Widget BasicEditor (Type editorType, object value,
					   object obj, PropertyDescriptor prop)
		{
			PropertyInfo vprop = ValueProperty (editorType);
			EventInfo vevent = ValueEvent (editorType);

			ConstructorInfo ctor;
			Widget editor;

			// Find a constructor. First we look for a constructor that takes
			// just an object of the editor's Value type (ie, a default
			// value). If that fails, we look for a no-arg constructor. If
			// there's no no-arg constructor, we look for a constructor that
			// takes a System.Type arg and call that with the type of the
			// property (this is used by the enum editor, which needs to know
			// specifically what kind of enum it's editing).

			ctor = editorType.GetConstructor (new Type[] { vprop.PropertyType });
			if (ctor != null)
				editor = (Widget)ctor.Invoke (new object[] { value });
			else {
				ctor = editorType.GetConstructor (new Type[0]);
				if (ctor != null)
					editor = (Widget)ctor.Invoke (new object[0]);
				else {
					ctor = editorType.GetConstructor (new Type[] { typeof (Type) });
					if (ctor == null)
						return null;
					editor = (Widget)ctor.Invoke (new object[] { prop.PropertyType });
				}

				// Set the default value
				if (vprop.CanWrite)
					vprop.SetValue (editor, value, null);
			}

			editor.ShowAll ();

			if (prop.CanWrite && (vevent != null)) {
				BasicEditorHelper beh = new BasicEditorHelper (prop, obj, vprop, editor);
				vevent.AddEventHandler (editor, new EventHandler (beh.ValueChanged));
			} else
				editor.Sensitive = false;

			return editor;
		}

		private class RangeEditorHelper {
			PropertyDescriptor prop;
			PropertyInfo vprop;
			object editor, obj;

			public RangeEditorHelper (PropertyDescriptor prop, object obj,
						  PropertyInfo vprop, object editor)
			{
				this.prop = prop;
				this.obj = obj;
				this.vprop = vprop;
				this.editor = editor;
			}

			public void ValueChanged (object o, EventArgs args)
			{
				prop.SetValue (obj, Convert.ChangeType (vprop.GetValue (editor, null), prop.PropertyType));
			}
		}

		static Widget RangeEditor (Type editorType, object min, object max, object initial,
					   object obj, PropertyDescriptor prop)
		{
			PropertyInfo vprop = ValueProperty (editorType);
			EventInfo vevent = ValueEvent (editorType);

			ConstructorInfo ctor;
			Widget editor;

			// If the passed in values aren't the same type as used by the
			// editor, convert them.
			if (vprop.PropertyType != prop.PropertyType) {
				min = Convert.ChangeType (min, vprop.PropertyType);
				max = Convert.ChangeType (max, vprop.PropertyType);
				initial = Convert.ChangeType (initial, vprop.PropertyType);
			}

			// First look for a constructor that takes a minimum, a maximum,
			// and a default value. Then look for one that just takes a minimum
			// and maximum.
			ctor = editorType.GetConstructor (new Type[] { vprop.PropertyType, vprop.PropertyType, vprop.PropertyType });
			if (ctor != null) {
				editor = (Widget)ctor.Invoke (new object[] { min, max, initial });
			} else {
				// min, max
				ctor = editorType.GetConstructor (new Type[] { vprop.PropertyType, vprop.PropertyType });
				if (ctor != null) {
					editor = (Widget)ctor.Invoke (new object[] { min, max });
					if (vprop.CanWrite)
						vprop.SetValue (editor, initial, null);
				} else
					return null;
			}

			editor.ShowAll ();

			if (prop.CanWrite && (vevent != null)) {
				RangeEditorHelper reh = new RangeEditorHelper (prop, obj, vprop, editor);
				vevent.AddEventHandler (editor, new EventHandler (reh.ValueChanged));
			} else
				editor.Sensitive = false;

			return editor;
		}

		static PropertyInfo ValueProperty (Type editorType)
		{
			if (editorType.IsDefined (typeof (Stetic.PropertyEditorAttribute), false)) {
				foreach (object attr in editorType.GetCustomAttributes (typeof (Stetic.PropertyEditorAttribute), false)) {
					PropertyEditorAttribute peattr = (PropertyEditorAttribute)attr;
					return editorType.GetProperty (peattr.Property);
				}
			}

			return editorType.GetProperty ("Value");
		}

		static EventInfo ValueEvent (Type editorType)
		{
			if (editorType.IsDefined (typeof (Stetic.PropertyEditorAttribute), false)) {
				foreach (object attr in editorType.GetCustomAttributes (typeof (Stetic.PropertyEditorAttribute), false)) {
					PropertyEditorAttribute peattr = (PropertyEditorAttribute)attr;
					return editorType.GetEvent (peattr.Event);
				}
			}

			return editorType.GetEvent ("ValueChanged");
		}

		static Widget NoEditor (PropertyDescriptor prop)
		{
			return new Gtk.Label ("(" + prop.PropertyType.Name + ")");
		}
	}
}

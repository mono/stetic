using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public abstract class PropertyEditor : VBox {
		protected PropertyEditor () : base (false, 0) {}

		public abstract void Update (object o, EventArgs args);

		static Hashtable editors;

		static PropertyEditor ()
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
			editors[typeof (Gdk.Color)] = typeof (Stetic.Editor.Color);
		}

		static public PropertyEditor MakeEditor (PropertyDescriptor prop, ParamSpec pspec, object obj)
		{
			Type editorType;
			object min, max, initial;

			Stetic.Wrapper.Object wrapper = Stetic.Wrapper.Object.Lookup (obj as GLib.Object);
			if (wrapper != null)
				obj = wrapper;

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
						return new NoEditor (prop);
				}
			}

			if (min != null && max != null)
				return new RangeEditor (editorType, min, max, initial, obj, prop);
			else
				return new BasicEditor (editorType, initial, obj, prop);
		}

		protected static PropertyInfo EditorValueProperty (Type editorType)
		{
			if (editorType.IsDefined (typeof (Stetic.PropertyEditorAttribute), false)) {
				foreach (object attr in editorType.GetCustomAttributes (typeof (Stetic.PropertyEditorAttribute), false)) {
					PropertyEditorAttribute peattr = (PropertyEditorAttribute)attr;
					return editorType.GetProperty (peattr.Property);
				}
			}

			return editorType.GetProperty ("Value");
		}

		protected static EventInfo EditorValueEvent (Type editorType)
		{
			if (editorType.IsDefined (typeof (Stetic.PropertyEditorAttribute), false)) {
				foreach (object attr in editorType.GetCustomAttributes (typeof (Stetic.PropertyEditorAttribute), false)) {
					PropertyEditorAttribute peattr = (PropertyEditorAttribute)attr;
					return editorType.GetEvent (peattr.Event);
				}
			}

			return editorType.GetEvent ("ValueChanged");
		}

	}

	class BasicEditor : PropertyEditor {

		Widget editor;
		object obj;
		PropertyDescriptor prop;

		PropertyInfo editorProp;
		EventInfo editorEvent;

		public BasicEditor (Type editorType, object value,
				    object obj, PropertyDescriptor prop)
		{
			this.obj = obj;
			this.prop = prop;

			editorProp = EditorValueProperty (editorType);
			editorEvent = EditorValueEvent (editorType);

			// Find a constructor. First we look for a constructor that takes
			// just an object of the editor's Value type (ie, a default
			// value). If that fails, we look for a no-arg constructor. If
			// there's no no-arg constructor, we look for a constructor that
			// takes a System.Type arg and call that with the type of the
			// property (this is used by the enum editor, which needs to know
			// specifically what kind of enum it's editing).

			ConstructorInfo ctor = editorType.GetConstructor (new Type[] { editorProp.PropertyType });
			if (ctor != null)
				editor = (Widget)ctor.Invoke (new object[] { value });
			else {
				ctor = editorType.GetConstructor (new Type[0]);
				if (ctor != null)
					editor = (Widget)ctor.Invoke (new object[0]);
				else {
					ctor = editorType.GetConstructor (new Type[] { typeof (Type) });
					if (ctor == null)
						throw new ApplicationException ("No constructor for editor type " + editorType.ToString () + " for " + prop.Name);;
					editor = (Widget)ctor.Invoke (new object[] { prop.PropertyType });
				}

				// Set the default value
				if (editorProp.CanWrite)
					editorProp.SetValue (editor, value, null);
			}

			editor.ShowAll ();

			if (prop.CanWrite && (editorEvent != null))
				editorEvent.AddEventHandler (editor, new EventHandler (EditorValueChanged));
			else
				editor.Sensitive = false;

			Add (editor);
		}

		bool syncing = false;

		void EditorValueChanged (object o, EventArgs args)
		{
			if (!syncing) {
				syncing = true;
				prop.SetValue (obj, editorProp.GetValue (editor, null));
				syncing = false;
			}
		}

		public override void Update (object o, EventArgs args)
		{
			if (!syncing) {
				syncing = true;
				editorProp.SetValue (editor, prop.GetValue (obj), null);
				syncing = false;
			}
		}
	}

	class RangeEditor : PropertyEditor {

		Widget editor;
		object obj;
		PropertyDescriptor prop;

		PropertyInfo editorProp;
		EventInfo editorEvent;

		public RangeEditor (Type editorType, object min, object max, object initial,
				    object obj, PropertyDescriptor prop)
		{
			this.obj = obj;
			this.prop = prop;

			editorProp = EditorValueProperty (editorType);
			editorEvent = EditorValueEvent (editorType);

			// If the passed in values aren't the same type as used by the
			// editor, convert them.
			if (editorProp.PropertyType != prop.PropertyType) {
				min = Convert.ChangeType (min, editorProp.PropertyType);
				max = Convert.ChangeType (max, editorProp.PropertyType);
				initial = Convert.ChangeType (initial, editorProp.PropertyType);
			}

			// First look for a constructor that takes a minimum, a maximum,
			// and a default value. Then look for one that just takes a minimum
			// and maximum.
			ConstructorInfo ctor = editorType.GetConstructor (new Type[] { editorProp.PropertyType, editorProp.PropertyType, editorProp.PropertyType });
			if (ctor != null) {
				editor = (Widget)ctor.Invoke (new object[] { min, max, initial });
			} else {
				// min, max
				ctor = editorType.GetConstructor (new Type[] { editorProp.PropertyType, editorProp.PropertyType });
				if (ctor != null) {
					editor = (Widget)ctor.Invoke (new object[] { min, max });
					if (editorProp.CanWrite)
						editorProp.SetValue (editor, initial, null);
				} else
					throw new ApplicationException ("No constructor for editor type " + editorType.ToString ());;
			}

			editor.ShowAll ();

			if (prop.CanWrite && (editorEvent != null))
				editorEvent.AddEventHandler (editor, new EventHandler (EditorValueChanged));
			else
				editor.Sensitive = false;

			Add (editor);
		}

		bool updating = false;

		void EditorValueChanged (object o, EventArgs args)
		{
			if (!updating)
				prop.SetValue (obj, Convert.ChangeType (editorProp.GetValue (editor, null), prop.PropertyType));
		}

		public override void Update (object o, EventArgs args)
		{
			updating = true;
			editorProp.SetValue (editor, Convert.ChangeType (prop.GetValue (obj), editorProp.PropertyType), null);
			updating = false;
		}
	}

	class NoEditor : PropertyEditor {
		public NoEditor (PropertyDescriptor prop)
		{
			Label label = new Gtk.Label ("(" + prop.PropertyType.Name + ")");
			label.Show ();
			Add (label);
		}

		public override void Update (object o, EventArgs args)
		{
			;
		}
	}
}

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

		static public PropertyEditor MakeEditor (PropertyDescriptor prop, ObjectWrapper wrapper)
		{
			Type editorType;
			object min, max;

			min = prop.Minimum;
			max = prop.Maximum;
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
				return new RangeEditor (editorType, min, max, wrapper, prop);
			else
				return new BasicEditor (editorType, wrapper, prop);
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
		ObjectWrapper wrapper;
		PropertyDescriptor prop;

		PropertyInfo editorProp;
		EventInfo editorEvent;

		public BasicEditor (Type editorType, ObjectWrapper wrapper, PropertyDescriptor prop)
		{
			this.wrapper = wrapper;
			this.prop = prop;

			editorProp = EditorValueProperty (editorType);
			editorEvent = EditorValueEvent (editorType);

			// Find a constructor.

			ConstructorInfo ctor = editorType.GetConstructor (new Type[] { typeof (PropertyInfo) });
			if (ctor != null)
				editor = (Widget)ctor.Invoke (new object[] { prop.PropertyInfo });
			else {
				ctor = editorType.GetConstructor (new Type[0]);
				if (ctor == null)
					throw new ApplicationException ("No constructor for editor type " + editorType.ToString () + " for " + prop.Name);;
				editor = (Widget)ctor.Invoke (new object[0]);
			}

			if (editorProp.CanWrite)
				editorProp.SetValue (editor, prop.GetValue (wrapper), null);

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
				prop.SetValue (wrapper, editorProp.GetValue (editor, null));
				syncing = false;
			}
		}

		public override void Update (object o, EventArgs args)
		{
			if (!syncing) {
				syncing = true;
				editorProp.SetValue (editor, prop.GetValue (wrapper), null);
				syncing = false;
			}
		}
	}

	class RangeEditor : PropertyEditor {

		Widget editor;
		ObjectWrapper wrapper;
		PropertyDescriptor prop;

		PropertyInfo editorProp;
		EventInfo editorEvent;

		public RangeEditor (Type editorType, object min, object max,
				    ObjectWrapper wrapper, PropertyDescriptor prop)
		{
			this.wrapper = wrapper;
			this.prop = prop;

			editorProp = EditorValueProperty (editorType);
			editorEvent = EditorValueEvent (editorType);

			object initial = prop.GetValue (wrapper);

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
				prop.SetValue (wrapper, Convert.ChangeType (editorProp.GetValue (editor, null), prop.PropertyType));
		}

		public override void Update (object o, EventArgs args)
		{
			updating = true;
			editorProp.SetValue (editor, Convert.ChangeType (prop.GetValue (wrapper), editorProp.PropertyType), null);
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

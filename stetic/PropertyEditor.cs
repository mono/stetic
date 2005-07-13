using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyEditor : VBox {
		Widget editor;
		object obj;
		PropertyDescriptor prop;

		PropertyInfo editorProp;
		EventInfo editorEvent;

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
			editors[typeof (string[])] = typeof (Stetic.Editor.StringArray);
			editors[typeof (Gdk.Color)] = typeof (Stetic.Editor.Color);
		}

		public PropertyEditor (PropertyDescriptor prop, object obj) : base (false, 0)
		{
			this.obj = obj;
			this.prop = prop;

			Type editorType;
			object min, max, initial;

			min = prop.Minimum;
			max = prop.Maximum;
			initial = prop.GetValue (obj);

			editorType = prop.EditorType;
			if (editorType == null) {
				if (prop.PropertyType.IsEnum) {
					editorType = typeof (Stetic.Editor.Enumeration);
					min = max = null;
				} else if (prop.PropertyType == typeof (int) && min != null && ((int)min == -1)) {
					editorType = typeof (Stetic.Editor.OptIntRange);
					min = 0;
				} else {
					editorType = editors[prop.PropertyType] as Type;
					if (editorType == null) {
						Label label = new Gtk.Label ("(" + prop.PropertyType.Name + ")");
						label.Show ();
						Add (label);
						syncing = true;
						return;
					}
				}
			}

			editorProp = EditorValueProperty (editorType);
			editorEvent = EditorValueEvent (editorType);

			// If the passed in values aren't the same type as used by the
			// editor, convert them.
			if (editorProp.PropertyType != prop.PropertyType && !prop.PropertyType.IsEnum) {
				if (min != null)
					min = Convert.ChangeType (min, editorProp.PropertyType);
				if (max != null)
					max = Convert.ChangeType (max, editorProp.PropertyType);
				if (initial != null)
					initial = Convert.ChangeType (initial, editorProp.PropertyType);
			}

			// Find a constructor.
			ConstructorInfo ctor = editorType.GetConstructor (new Type[] { typeof (PropertyInfo) });
			if (ctor != null)
				editor = (Widget)ctor.Invoke (new object[] { prop.PropertyInfo });
			else {
				// min/max
				ctor = editorType.GetConstructor (new Type[] { typeof (object), typeof (object) });
				if (ctor != null)
					editor = (Widget)ctor.Invoke (new object[] { min, max });
				else {
					ctor = editorType.GetConstructor (new Type[0]);
					if (ctor == null)
						throw new ApplicationException ("No constructor for editor type " + editorType.ToString () + " for " + prop.Name);;
					editor = (Widget)ctor.Invoke (new object[0]);
				}
			}

			if (editorProp.CanWrite && initial != null)
				editorProp.SetValue (editor, initial, null);

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
				object value = editorProp.GetValue (editor, null);
				if (editorProp.PropertyType != prop.PropertyType && !prop.PropertyType.IsEnum)
					value = Convert.ChangeType (value, prop.PropertyType);
				prop.SetValue (obj, value);
				syncing = false;
			}
		}

		public void Update ()
		{
			if (!syncing) {
				syncing = true;
				object value = prop.GetValue (obj);
				if (editorProp.PropertyType != prop.PropertyType && !prop.PropertyType.IsEnum)
					value = Convert.ChangeType (value, editorProp.PropertyType);
				editorProp.SetValue (editor, value, null);
				syncing = false;
			}
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
}

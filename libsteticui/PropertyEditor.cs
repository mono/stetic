using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyEditor : VBox {
		IPropertyEditor propEditor;
		
		object obj;
		PropertyDescriptor prop;

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
		
		public PropertyEditor (PropertyDescriptor prop) : base (false, 0)
		{
			this.prop = prop;
			
			propEditor = CreateEditor (prop);
			propEditor.ValueChanged += new EventHandler (EditorValueChanged);

			Add ((Gtk.Widget) propEditor);
		}

		public void AttachObject (object ob)
		{
			if (ob == null)
				throw new ArgumentNullException ("ob");
			
			syncing = true;
			this.obj = ob;
			propEditor.AttachObject (obj);
			
			// It is the responsibility of the editor to convert value types
			object initial = prop.GetValue (obj);
			propEditor.Value = initial;
			
			syncing = false;
		}
		
		public IPropertyEditor CreateEditor (PropertyDescriptor prop)
		{
			Type editorType;
			object min, max;
			
			IPropertyEditor editor = null;

			min = prop.Minimum;
			max = prop.Maximum;

			editorType = prop.EditorType;
			if (editorType == null) {
				if (prop.PropertyType.IsEnum) {
					if (prop.PropertyType.IsDefined (typeof (FlagsAttribute), true))
						editor = new Stetic.Editor.Flags ();
					else
						editor = new Stetic.Editor.Enumeration ();
				} else if (prop.PropertyType == typeof (int) && min != null && ((int)min == -1)) {
					editor = new Stetic.Editor.OptIntRange (0, max);
				} else {
					editorType = editors[prop.PropertyType] as Type;
					if (editorType == null)
						editor = new DummyEditor ();
				}
			}
			
			if (editor == null) {
				editor = Activator.CreateInstance (editorType) as IPropertyEditor;
				if (editor == null)
					throw new Exception ("The property editor '" + editorType + "' must implement the interface IPropertyEditor");
			}

			editor.Initialize (prop);

			Gtk.Widget w = editor as Gtk.Widget;
			if (w == null)
				throw new Exception ("The property editor '" + editorType + "' must be a Gtk Widget");
			w.ShowAll ();
			return editor;
		}

		bool syncing = false;

		void EditorValueChanged (object o, EventArgs args)
		{
			if (!syncing) {
				syncing = true;
				prop.SetValue (obj, propEditor.Value);
				syncing = false;
			}
		}

		public void Update ()
		{
			if (!syncing) {
				syncing = true;
				propEditor.Value = prop.GetValue (obj);
				syncing = false;
			}
		}
	}
	
	class DummyEditor: Gtk.Label, IPropertyEditor
	{
		public void Initialize (PropertyDescriptor prop)
		{
			Text = "(" + prop.PropertyType.Name + ")";
		}
		
		public void AttachObject (object obj)
		{
		}
		
		public object Value { 
			get { return null; } 
			set { }
		}
		
		public event EventHandler ValueChanged { add {} remove {} }
	}
}

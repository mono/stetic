using GLib;
using Gtk;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Stetic {

	public class PropertyDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		ParamSpec pspec;
		EventInfo eventInfo;
		Type editorType;
		object defaultValue;
		ArrayList dependencies = new ArrayList (), inverseDependencies = new ArrayList ();

		public PropertyDescriptor (Type objectType, string propertyName)
		{
			Type parentType;

			int dot = propertyName.IndexOf ('.');

			if (dot == -1) {
				parentType = objectType;
				memberInfo = null;
				propertyInfo = objectType.GetProperty (propertyName, BindingFlags.Public | BindingFlags.Instance);
			} else {
				memberInfo = objectType.GetProperty (propertyName.Substring (0, dot), BindingFlags.Public | BindingFlags.Instance);
				if (memberInfo == null)
					throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);
				parentType = memberInfo.PropertyType;
				propertyInfo = parentType.GetProperty (propertyName.Substring (dot + 1), BindingFlags.Public | BindingFlags.Instance);
			}
			if (propertyInfo == null)
				throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);

			eventInfo = parentType.GetEvent (propertyInfo.Name + "Changed", BindingFlags.Public | BindingFlags.Instance);

			// FIXME. This is ugly.
			if (objectType.GetInterface ("Stetic.IObjectWrapper") != null &&
			    objectType.BaseType.GetProperty (propertyName) != null) {
				PropertyDescriptor baseProp = new PropertyDescriptor (objectType.BaseType, propertyName);
				if (baseProp != null) {
					editorType = baseProp.editorType;
					defaultValue = baseProp.defaultValue;
					pspec = baseProp.pspec;
				}
			}

			foreach (object attr in propertyInfo.GetCustomAttributes (false)) {
				if (attr is System.ComponentModel.EditorAttribute) {
					EditorAttribute eattr = (EditorAttribute)attr;
					editorType = Type.GetType (eattr.EditorTypeName);
				}

				if (attr is System.ComponentModel.DefaultValueAttribute) {
					DefaultValueAttribute dvattr = (DefaultValueAttribute)attr;
					defaultValue = dvattr.Value;
				}

				if (attr is GLib.PropertyAttribute) {
					PropertyAttribute pattr = (PropertyAttribute)attr;
					pspec = ParamSpec.LookupObjectProperty (parentType, pattr.Name);
				}

				if (attr is Gtk.ChildPropertyAttribute) {
					ChildPropertyAttribute cpattr = (ChildPropertyAttribute)attr;
					pspec = ParamSpec.LookupChildProperty (parentType.DeclaringType, cpattr.Name);
				}
			}
		}

		public string Name {
			get {
				return propertyInfo.Name;
			}
		}

		public PropertyInfo Info {
			get {
				return propertyInfo;
			}
		}

		public Type PropertyType {
			get {
				return propertyInfo.PropertyType;
			}
		}

		public EventInfo EventInfo {
			get {
				return eventInfo;
			}
		}

		public ParamSpec ParamSpec {
			get {
				return pspec;
			}
		}

		public Type EditorType {
			get {
				return editorType;
			}
		}

		public object Default {
			get {
				return defaultValue;
			}
		}

		public object PropertyObject (object obj)
		{
			return memberInfo == null ? obj : memberInfo.GetValue (obj, null);
		}

		public bool CanRead {
			get {
				return propertyInfo.CanRead;
			}
		}

		public object GetValue (object obj)
		{
			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			return propertyInfo.GetValue (obj, null);
		}

		public bool CanWrite {
			get {
				return propertyInfo.CanWrite;
			}
		}

		public void SetValue (object obj, object value)
		{
			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			propertyInfo.SetValue (obj, value, null);
		}

		public void DependsOn (PropertyDescriptor master)
		{
			dependencies.Add (master);
		}

		public void DependsInverselyOn (PropertyDescriptor master)
		{
			inverseDependencies.Add (master);
		}

		public IList Dependencies {
			get {
				return dependencies;
			}
		}

		public IList InverseDependencies {
			get {
				return inverseDependencies;
			}
		}

	}
}

using Gtk;
using Gdk;
using GLib;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Stetic {

	public class PropertyDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		ParamSpec pspec;
		EventInfo eventInfo;
		Type editorType;
		object defaultValue;

		public PropertyDescriptor (Type objectType, Type baseType, string propertyName)
		{
			Type parentType;

			int dot = propertyName.IndexOf ('.');

			if (dot == -1) {
				parentType = objectType;
				memberInfo = null;
				propertyInfo = objectType.GetProperty (propertyName);
			} else {
				memberInfo = objectType.GetProperty (propertyName.Substring (0, dot));
				if (memberInfo == null)
					throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);
				parentType = memberInfo.PropertyType;
				propertyInfo = parentType.GetProperty (propertyName.Substring (dot + 1));
			}
			if (propertyInfo == null)
				throw new ArgumentException ("Invalid property name " + parentType.Name + "." + propertyName);

			eventInfo = parentType.GetEvent (propertyInfo.Name + "Changed");

			if (baseType != null) {
				PropertyDescriptor baseProp = new PropertyDescriptor (baseType, propertyName);
				editorType = baseProp.editorType;
				defaultValue = baseProp.defaultValue;
				pspec = baseProp.pspec;
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

		public PropertyDescriptor (Type objectType, string propertyName) : this (objectType, null, propertyName) {}

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
	}
}

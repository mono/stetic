using Gtk;
using Gdk;
using GLib;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Stetic {

	public class PropertyDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		Type editorType;
		object defaultValue;

		public PropertyDescriptor (Type objectType, string propertyName)
		{
			int dot = propertyName.IndexOf ('.');

			if (dot == -1) {
				memberInfo = null;
				propertyInfo = objectType.GetProperty (propertyName);
			} else {
				memberInfo = objectType.GetProperty (propertyName.Substring (0, dot));
				propertyInfo = memberInfo.PropertyType.GetProperty (propertyName.Substring (dot + 1));
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

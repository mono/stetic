using GLib;
using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyDescriptor : ItemDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		bool isWrapperProperty;
		ParamSpec pspec;
		Type editorType;
		object minimum, maximum;

		public PropertyDescriptor (Type objectType, string propertyName) : this (null, objectType, propertyName) {}

		public PropertyDescriptor (Type wrapperType, Type objectType, string propertyName)
		{
			Type trueObjectType;
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

			int dot = propertyName.IndexOf ('.');

			if (dot == -1) {
				trueObjectType = objectType;
				memberInfo = null;
				if (wrapperType != null)
					propertyInfo = wrapperType.GetProperty (propertyName, flags);
				if (propertyInfo == null)
					propertyInfo = objectType.GetProperty (propertyName, flags);
				else
					isWrapperProperty = true;
			} else {
				if (wrapperType != null)
					memberInfo = wrapperType.GetProperty (propertyName.Substring (0, dot), flags);
				if (memberInfo == null)
					memberInfo = objectType.GetProperty (propertyName.Substring (0, dot), flags);
				else
					isWrapperProperty = true;
				if (memberInfo == null)
					throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);
				trueObjectType = memberInfo.PropertyType;
				propertyInfo = trueObjectType.GetProperty (propertyName.Substring (dot + 1), flags);
			}
			if (propertyInfo == null)
				throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);

			// FIXME, this is sort of left over from the old code. We should
			// do it nicer.
			if (wrapperType != null) {
				try {
					PropertyDescriptor baseProp = new PropertyDescriptor (objectType, propertyName);
					if (baseProp != null)
						pspec = baseProp.pspec;
				} catch {
					;
				}
			}

			foreach (object attr in propertyInfo.GetCustomAttributes (false)) {
				if (attr is Stetic.EditorAttribute) {
					EditorAttribute eattr = (EditorAttribute)attr;
					editorType = eattr.EditorType;
				}

				if (attr is Stetic.RangeAttribute) {
					RangeAttribute rattr = (RangeAttribute)attr;
					minimum = rattr.Minimum;
					maximum = rattr.Maximum;
				}

				if (attr is GLib.PropertyAttribute) {
					PropertyAttribute pattr = (PropertyAttribute)attr;
					pspec = ParamSpec.LookupObjectProperty (trueObjectType, pattr.Name);
				}

				if (attr is Gtk.ChildPropertyAttribute) {
					ChildPropertyAttribute cpattr = (ChildPropertyAttribute)attr;
					pspec = ParamSpec.LookupChildProperty (trueObjectType.DeclaringType, cpattr.Name);
				}
			}
		}

		public override string Name {
			get {
				return propertyInfo.Name;
			}
		}

		public Type PropertyType {
			get {
				return propertyInfo.PropertyType;
			}
		}

		public PropertyInfo PropertyInfo {
			get {
				return propertyInfo;
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

		public object Minimum {
			get {
				return minimum;
			}
		}

		public object Maximum {
			get {
				return maximum;
			}
		}

		// Gets the value of the property on obj
		public object GetValue (object obj)
		{
			Stetic.Wrapper.Object wrapper = obj as Stetic.Wrapper.Object;
			if (wrapper != null && !isWrapperProperty)
				obj = wrapper.Wrapped;
			else if (wrapper == null && isWrapperProperty)
				throw new ApplicationException ("Requested wrapper property " + propertyInfo.Name + " on non-wrapper object " + obj.ToString ());

			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			return propertyInfo.GetValue (obj, null);
		}

		public bool CanWrite {
			get {
				return propertyInfo.CanWrite;
			}
		}

		// Sets the value of the property on obj to value
		public void SetValue (object obj, object value)
		{
			Stetic.Wrapper.Object wrapper = obj as Stetic.Wrapper.Object;
			if (wrapper != null && !isWrapperProperty)
				obj = wrapper.Wrapped;

			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			propertyInfo.SetValue (obj, value, null);
		}
	}
}

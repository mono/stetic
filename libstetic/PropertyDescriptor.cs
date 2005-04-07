using GLib;
using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyDescriptor : ItemDescriptor {

		PropertyInfo memberInfo, propertyInfo, baseInfo;
		bool isWrapperProperty;
		ParamSpec pspec;
		Type editorType;
		string label, description;
		object minimum, maximum;
		GladePropertyAttribute gladeAttribute;
		PropertyDescriptor gladeProperty;

		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public PropertyDescriptor (Type wrapperType, Type objectType, string propertyName)
		{
			int dot = propertyName.IndexOf ('.');
			int slash = propertyName.IndexOf ('/');

			if (dot != -1) {
				// Sub-property (eg, "Alignment.Value")
				memberInfo = FindProperty (wrapperType, objectType, propertyName.Substring (0, dot));
				isWrapperProperty = memberInfo.DeclaringType.IsSubclassOf (typeof (ObjectWrapper));
				gladeProperty = new PropertyDescriptor (wrapperType, objectType, memberInfo.Name);
				propertyInfo = FindProperty (memberInfo.PropertyType, propertyName.Substring (dot + 1));
			} else if (slash != -1) {
				// Split-up property (eg, "XOptions/XExpand")
				gladeProperty = new PropertyDescriptor (wrapperType, objectType, propertyName.Substring (0, slash));
				propertyInfo = FindProperty (wrapperType, objectType, propertyName.Substring (slash + 1));
				isWrapperProperty = true;
			} else {
				// Basic simple property
				propertyInfo = FindProperty (wrapperType, objectType, propertyName);
				isWrapperProperty = propertyInfo.DeclaringType.IsSubclassOf (typeof (ObjectWrapper));
			}

			pspec = FindPSpec (propertyInfo);
			if (isWrapperProperty && pspec == null) {
				PropertyInfo pinfo = objectType.GetProperty (propertyInfo.Name, flags);
				if (pinfo != null)
					pspec = FindPSpec (pinfo);
			}

			if (pspec != null) {
				label = pspec.Nick;
				description = pspec.Blurb;
				minimum = pspec.Minimum;
				maximum = pspec.Maximum;
			} else
				label = propertyInfo.Name;

			foreach (object attr in propertyInfo.GetCustomAttributes (false)) {
				if (attr is Stetic.DescriptionAttribute) {
					DescriptionAttribute dattr = (DescriptionAttribute)attr;
					label = dattr.Name;
					description = dattr.Description;
				}

				if (attr is Stetic.EditorAttribute) {
					EditorAttribute eattr = (EditorAttribute)attr;
					editorType = eattr.EditorType;
				}

				if (attr is Stetic.RangeAttribute) {
					RangeAttribute rattr = (RangeAttribute)attr;
					minimum = rattr.Minimum;
					maximum = rattr.Maximum;
				}

				if (attr is Stetic.GladePropertyAttribute) {
					gladeAttribute = (GladePropertyAttribute)attr;
					if (gladeAttribute.Proxy != null)
						gladeProperty = new PropertyDescriptor (wrapperType, objectType, gladeAttribute.Proxy);
				}
			}
		}

		static PropertyInfo FindProperty (Type type, string propertyName) {
			return FindProperty (null, type, propertyName);
		}

		static PropertyInfo FindProperty (Type wrapperType, Type objectType, string propertyName)
		{
			PropertyInfo info;

			if (wrapperType != null) {
				info = wrapperType.GetProperty (propertyName, flags);
				if (info != null)
					return info;
			}

			info = objectType.GetProperty (propertyName, flags);
			if (info != null)
				return info;

			throw new ArgumentException ("Invalid property name " + objectType.Name + "." + propertyName);
		}

		ParamSpec FindPSpec (PropertyInfo pinfo)
		{
			foreach (object attr in pinfo.GetCustomAttributes (false)) {
				if (attr is GLib.PropertyAttribute) {
					PropertyAttribute pattr = (PropertyAttribute)attr;
					return ParamSpec.LookupObjectProperty (pinfo.DeclaringType, pattr.Name);
				}

				if (attr is Gtk.ChildPropertyAttribute) {
					ChildPropertyAttribute cpattr = (ChildPropertyAttribute)attr;
					return ParamSpec.LookupChildProperty (pinfo.DeclaringType.DeclaringType, cpattr.Name);
				}
			}
			return null;
		}

		// The property's internal name
		public override string Name {
			get {
				return propertyInfo.Name;
			}
		}

		// The property's user-visible name
		public string Label {
			get {
				return label;
			}
		}

		// The property's user-visible description
		public string Description {
			get {
				return description;
			}
		}

		// The property's type
		public Type PropertyType {
			get {
				return propertyInfo.PropertyType;
			}
		}

		// The property's PropertyInfo
		public PropertyInfo PropertyInfo {
			get {
				return propertyInfo;
			}
		}

		// The property's ParamSpec
		public ParamSpec ParamSpec {
			get {
				return pspec;
			}
		}

		// The property's GUI editor type, if overridden
		public Type EditorType {
			get {
				return editorType;
			}
		}

		// The property's minimum value, if declared
		public object Minimum {
			get {
				return minimum;
			}
		}

		// The property's maximum value, if declared
		public object Maximum {
			get {
				return maximum;
			}
		}

		// Gets the value of the property on @wrapper
		public object GetValue (ObjectWrapper wrapper)
		{
			object obj = isWrapperProperty ? wrapper : wrapper.Wrapped;
			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			return propertyInfo.GetValue (obj, null);
		}

		// Whether or not the property is writable
		public bool CanWrite {
			get {
				return propertyInfo.CanWrite;
			}
		}

		// Sets the value of the property on @wrapper
		public void SetValue (ObjectWrapper wrapper, object value)
		{
			object obj = isWrapperProperty ? wrapper : wrapper.Wrapped;
			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			propertyInfo.SetValue (obj, value, null);
		}

		public GladeProperty GladeFlags {
			get {
				return gladeAttribute != null ? gladeAttribute.Flags : 0;
			}
		}

		public PropertyDescriptor GladeProperty {
			get {
				return gladeProperty;
			}
		}

		public string GladeName {
			get {
				if (gladeAttribute != null && gladeAttribute.Name != null)
					return gladeAttribute.Name;
				else if (gladeProperty != null && gladeProperty.GladeName != null)
					return gladeProperty.GladeName;
				else if (pspec != null)
					return pspec.Name.Replace ('-', '_');
				else
					return null;
			}
		}

		public object GladeGetValue (ObjectWrapper wrapper)
		{
			if (gladeProperty != null)
				return gladeProperty.GetValue (wrapper);
			else {
				object obj = isWrapperProperty ? wrapper : wrapper.Wrapped;
				if (memberInfo != null)
					return memberInfo.GetValue (obj, null);
				else
					return propertyInfo.GetValue (obj, null);
			}
		}

		public void GladeSetValue (ObjectWrapper wrapper, object value)
		{
			if (gladeProperty != null)
				gladeProperty.SetValue (wrapper, value);
			else {
				object obj = isWrapperProperty ? wrapper : wrapper.Wrapped;
				if (memberInfo != null)
					memberInfo.SetValue (obj, value, null);
				else
					propertyInfo.SetValue (obj, value, null);
			}
		}
	}
}

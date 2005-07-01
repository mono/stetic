using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Stetic {

	public class PropertyDescriptor : ItemDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		bool isWrapperProperty, hasDefault;
		ParamSpec pspec;
		Type editorType;
		string label, description;
		object minimum, maximum;
		GladePropertyAttribute gladeAttribute;
		PropertyDescriptor gladeProperty;

		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public PropertyDescriptor (XmlElement elem, ItemGroup group, ClassDescriptor klass) : base (elem, group, klass)
		{
			string propertyName = elem.GetAttribute ("name");
			int dot = propertyName.IndexOf ('.');

			if (dot != -1) {
				// Sub-property (eg, "Alignment.Value")
				memberInfo = FindProperty (klass.WrapperType, klass.WrappedType, propertyName.Substring (0, dot));
				isWrapperProperty = memberInfo.DeclaringType.IsSubclassOf (typeof (ObjectWrapper));
				gladeProperty = new PropertyDescriptor (isWrapperProperty ? klass.WrapperType : klass.WrappedType, memberInfo.Name);
				propertyInfo = FindProperty (memberInfo.PropertyType, propertyName.Substring (dot + 1));
			} else {
				// Basic simple property
				propertyInfo = FindProperty (klass.WrapperType, klass.WrappedType, propertyName);
				isWrapperProperty = propertyInfo.DeclaringType.IsSubclassOf (typeof (ObjectWrapper));
			}

			pspec = FindPSpec (propertyInfo);
			if (isWrapperProperty && pspec == null) {
				PropertyInfo pinfo = klass.WrappedType.GetProperty (propertyInfo.Name, flags);
				if (pinfo != null)
					pspec = FindPSpec (pinfo);
			}

			if (elem.HasAttribute ("label"))
				label = elem.GetAttribute ("label");
			else if (pspec != null)
				label = pspec.Nick;
			else
				label = propertyInfo.Name;

			if (elem.HasAttribute ("description"))
				description = elem.GetAttribute ("description");
			else if (pspec != null)
				description = pspec.Blurb;

			if (elem.HasAttribute ("min"))
				minimum = Convert.ChangeType (elem.GetAttribute ("min"), propertyInfo.PropertyType);
			else if (pspec != null)
				minimum = pspec.Minimum;

			if (elem.HasAttribute ("max"))
				maximum = Convert.ChangeType (elem.GetAttribute ("max"), propertyInfo.PropertyType);
			else if (pspec != null)
				maximum = pspec.Maximum;

			if (pspec != null && !elem.HasAttribute ("ignore-default"))
				hasDefault = true;

			editorType = Type.GetType (elem.GetAttribute ("editor"));

			gladeAttribute = (GladePropertyAttribute) Attribute.GetCustomAttribute (propertyInfo, typeof (Stetic.GladePropertyAttribute));
			if (gladeAttribute != null && gladeAttribute.Proxy != null) {
				gladeProperty = new PropertyDescriptor (klass.WrapperType, gladeAttribute.Proxy);
				if (gladeProperty.pspec == null)
					gladeProperty.pspec = pspec;
				if (gladeProperty.gladeAttribute == null)
					gladeProperty.gladeAttribute = gladeAttribute;
			}
		}

		PropertyDescriptor (Type objectType, string propertyName)
		{
			propertyInfo = FindProperty (objectType, propertyName);
			isWrapperProperty = false;

			pspec = FindPSpec (propertyInfo);
			if (pspec != null) {
				label = pspec.Nick;
				description = pspec.Blurb;
				minimum = pspec.Minimum;
				maximum = pspec.Maximum;
				hasDefault = true;
			} else
				label = propertyInfo.Name;
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
					GLib.PropertyAttribute pattr = (GLib.PropertyAttribute)attr;
					return ParamSpec.LookupObjectProperty (pinfo.DeclaringType, pattr.Name);
				}

				if (attr is Gtk.ChildPropertyAttribute) {
					Gtk.ChildPropertyAttribute cpattr = (Gtk.ChildPropertyAttribute)attr;
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

		// Whether or not the property has a default value
		public bool HasDefault {
			get {
				return hasDefault;
			}
			set {
				hasDefault = value;
			}
		}

		// Gets the value of the property on @obj
		public object GetValue (object obj)
		{
			if (isWrapperProperty)
				obj = ObjectWrapper.Lookup (obj);
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

		// Sets the value of the property on @obj
		public void SetValue (object obj, object value)
		{
			if (isWrapperProperty)
				obj = ObjectWrapper.Lookup (obj);
			if (memberInfo != null)
				obj = memberInfo.GetValue (obj, null);
			propertyInfo.SetValue (obj, value, null);
		}

		public GladeProperty GladeFlags {
			get {
				if (gladeAttribute != null)
					return gladeAttribute.Flags;
				else
					return 0;
			}
		}

		public PropertyDescriptor GladeProperty {
			get {
				if (gladeProperty != null)
					return gladeProperty;
				else
					return this;
			}
		}

		public string GladeName {
			get {
				if (gladeAttribute != null && gladeAttribute.Name != null)
					return gladeAttribute.Name;
				else if (pspec != null)
					return pspec.Name.Replace ('-', '_');
				else
					return null;
			}
		}
	}
}

using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Stetic {

	public class TranslationInfo {
		public bool Translated;
		public string Context, Comment;
	}

	public class PropertyDescriptor : ItemDescriptor {

		PropertyInfo memberInfo, propertyInfo;
		bool isWrapperProperty, hasDefault, gladeOverride, initWithName;
		ParamSpec pspec;
		Type editorType;
		string label, description, gladeName;
		object minimum, maximum;
		PropertyDescriptor gladeProperty;

		Hashtable translationInfo;

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

			if (elem.HasAttribute ("glade-override"))
				gladeOverride = true;
			else
				gladeOverride = (pspec == null);

			if (elem.HasAttribute ("glade-name"))
				gladeName = elem.GetAttribute ("glade-name");

			if (elem.HasAttribute ("init-with-name"))
				initWithName = true;

			if (elem.HasAttribute ("translatable"))
				translationInfo = new Hashtable ();
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

		public bool GladeOverride {
			get {
				return gladeOverride;
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
				if (gladeName != null)
					return gladeName;
				else if (pspec != null)
					return pspec.Name.Replace ('-', '_');
				else
					return null;
			}
		}

		public bool InitWithName {
			get {
				return initWithName;
			}
		}

		public bool Translatable {
			get {
				return translationInfo != null;
			}
		}

		public bool IsTranslated (object obj)
		{
			TranslationInfo info = (TranslationInfo)translationInfo[obj];

			// Since translatable properties are assumed to be translated
			// by default, we return true if there is no TranslationInfo
			// for the object
			return (info == null || info.Translated == true);
		}

		public void SetTranslated (object obj, bool translated)
		{
			TranslationInfo info = (TranslationInfo)translationInfo[obj];
			if (info == null) {
				info = new TranslationInfo ();
				translationInfo[obj] = info;
			}

			if (translated)
				info.Translated = true;
			else
				info.Translated = false;
			// We leave the old Context and Comment around, so that if
			// you toggle Translated off and then back on, the old info
			// is still there.
		}

		public string TranslationContext (object obj)
		{
			TranslationInfo info = (TranslationInfo)translationInfo[obj];

			return info != null ? info.Context : null;
		}

		public void SetTranslationContext (object obj, string context)
		{
			SetTranslated (obj, true);
			((TranslationInfo)translationInfo[obj]).Context = context;
		}

		public string TranslationComment (object obj)
		{
			TranslationInfo info = (TranslationInfo)translationInfo[obj];

			return info != null ? info.Comment : null;
		}

		public void SetTranslationComment (object obj, string comment)
		{
			SetTranslated (obj, true);
			((TranslationInfo)translationInfo[obj]).Comment = comment;
		}
	}
}

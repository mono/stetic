using System;

namespace Stetic {

	public enum ObjectWrapperType {
		Object,
		Widget,
		Container,
		Window
	};

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ObjectWrapperAttribute : Attribute {
		string name, iconName;
		ObjectWrapperType type;
		Type wrappedType;

		[Translatable]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string IconName {
			get { return iconName; }
			set { iconName = value; }
		}

		public ObjectWrapperType Type {
			get { return type; }
			set { type = value; }
		}

		public Type WrappedType {
			get { return wrappedType; }
			set { wrappedType = value; }
		}

		public ObjectWrapperAttribute (string name, string iconName, Type wrappedType, ObjectWrapperType type)
		{
			this.name = name;
			this.iconName = iconName;
			this.type = type;
			this.wrappedType = wrappedType;
		}
	}

}

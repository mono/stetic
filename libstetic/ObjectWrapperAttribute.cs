using System;

namespace Stetic {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class ObjectWrapperAttribute : Attribute {
		string name, iconName, type;
		bool deprecated;

		[Translatable]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string IconName {
			get { return iconName; }
			set { iconName = value; }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public bool Deprecated {
			get { return deprecated; }
			set { deprecated = value; }
		}

		public ObjectWrapperAttribute (string name, string iconName, string type)
		{
			this.name = name;
			this.iconName = iconName;
			this.type = type;
		}
	}

}

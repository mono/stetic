using System;

namespace Stetic {

	public enum WidgetType {
		Normal,
		Container,
		Window
	};

	public class WidgetWrapperAttribute : Attribute {
		string name, iconName;
		WidgetType type;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string IconName {
			get { return iconName; }
			set { iconName = value; }
		}

		public WidgetType Type {
			get { return type; }
			set { type = value; }
		}

		public WidgetWrapperAttribute (string name, string iconName, WidgetType type)
		{
			this.name = name;
			this.iconName = iconName;
			this.type = type;
		}

		public WidgetWrapperAttribute (string name, string iconName) : this (name, iconName, WidgetType.Normal) {}
	}

}

using System;

namespace Stetic {

	public class WidgetWrapperAttribute : Attribute {
		string name, iconName;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string IconName {
			get { return iconName; }
			set { iconName = value; }
		}

		public WidgetWrapperAttribute (string name, string iconName)
		{
			this.name = name;
			this.iconName = iconName;
		}
	}

}

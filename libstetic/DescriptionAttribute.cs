using System;

namespace Stetic {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class DescriptionAttribute : Attribute {
		string name, description;

		public DescriptionAttribute (string name, string description)
		{
			this.name = name;
			this.description = description;
		}

		[Translatable]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[Translatable]
		public string Description {
			get { return description; }
			set { description = value; }
		}
	}
}

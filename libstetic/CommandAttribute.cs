using System;

namespace Stetic {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class CommandAttribute : Attribute {
		string name, description, checker;

		public CommandAttribute (string name, string description)
		{
			this.name = name;
			this.description = description;
		}

		public CommandAttribute (string name, string description, string checker)
		{
			this.name = name;
			this.description = description;
			this.checker = checker;
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

		public string Checker {
			get { return checker; }
			set { checker = value; }
		}
	}
}

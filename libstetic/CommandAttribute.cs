using System;

namespace Stetic {

	public class CommandAttribute : Attribute {
		string label, checker;

		public CommandAttribute (string label)
		{
			this.label = label;
		}

		public CommandAttribute (string label, string checker)
		{
			this.label = label;
			this.checker = checker;
		}

		public string Label {
			get { return label; }
			set { label = value; }
		}

		public string Checker {
			get { return checker; }
			set { checker = value; }
		}
	}
}

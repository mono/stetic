using System;

namespace Stetic {

	public class GladeException : ApplicationException {

		public GladeException (string message) : base (message) { }

		public GladeException (string message, string className) : this (message)
		{
			this.className = className;
		}

		public GladeException (string message, string className,
				       bool childprop, string propName, string propVal) : this (message, className)
		{
			this.childprop = childprop;
			this.propName = propName;
			this.propVal = propVal;
		}

		string className, propName, propVal;
		bool childprop;

		public string ClassName {
			get {
				return className;
			}
			set {
				className = value;
			}
		}

		public bool ChildProp {
			get {
				return childprop;
			}
			set {
				childprop = value;
			}
		}

		public string PropName {
			get {
				return propName;
			}
			set {
				propName = value;
			}
		}

		public string PropVal {
			get {
				return propVal;
			}
			set {
				propVal = value;
			}
		}

		public override string ToString ()
		{
			if (className == null)
				return Message;
			else if (propName == null)
				return Message + " (class " + className + ")";
			else if (propVal == null)
				return Message + " (class " + className + ", " + (childprop ? "child " : "") + "property " + propName + ")";
			else
				return Message + " (class " + className + ", " + (childprop ? "child " : "") + "property " + propName + ", value " + propVal + ")";
		}
	}
}

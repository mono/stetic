using System;

namespace Stetic {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class RangeAttribute : Attribute {

		public RangeAttribute (object min, object max)
		{
			this.min = min;
			this.max = max;
		}

		public RangeAttribute (object min, object max, object epsilon) : this (min, max)
		{
			this.epsilon = epsilon;
		}

		object min;
		public object Minimum {
			get { return min; }
			set { min = value; }
		}

		object max;
		public object Maximum {
			get { return max; }
			set { max = value; }
		}

		object epsilon;
		public object Epsilon {
			get { return epsilon; }
			set { epsilon = value; }
		}
	}
}

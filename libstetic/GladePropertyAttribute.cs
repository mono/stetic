using System;

namespace Stetic {

	[Flags]
	public enum GladeProperty {
		Wrapped       = (1 << 0),
		Proxied       = (1 << 1),
		LateImport    = (1 << 2),
		UseUnderlying = (1 << 3)
	}

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class GladePropertyAttribute : Attribute {

		public GladePropertyAttribute () : this (0) { }
		public GladePropertyAttribute (GladeProperty flags) 
		{
			this.flags = flags | GladeProperty.Wrapped;
		}

		GladeProperty flags;
		public GladeProperty Flags {
			get { return flags; }
		}

		string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}

		string proxy;
		public string Proxy {
			get { return proxy; }
			set {
				proxy = value;
				if (proxy != null)
					flags |= GladeProperty.Proxied;
				else
					flags &= ~GladeProperty.Proxied;
			}
		}
	}
}

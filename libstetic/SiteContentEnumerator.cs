using Gtk;
using System;
using System.Collections;

namespace Stetic {
	public class SiteContentEnumerator : IEnumerator, IEnumerable {
		Widget[] children;
		int i;
		object current;

		public SiteContentEnumerator (Gtk.Container container)
		{
			children = container.Children;
			i = -1;
		}

		public IEnumerator GetEnumerator () {
			return this;
		}

		public void Reset ()
		{
			i = -1;
		}

		public bool MoveNext ()
		{
			for (i++; i < children.Length; i++) {
				if (children[i] is WidgetSite) {
					current = ((WidgetSite)children[i]).Contents;
					if (current != null)
						return true;
				} else if (children[i] is IWidgetWrapper) {
					current = children[i];
					return true;
				}
			}
			return false;
		}

		public object Current {
			get {
				return current;
			}
		}
	}
}


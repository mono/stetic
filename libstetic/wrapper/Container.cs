using System;
using System.Collections;

namespace Stetic.Wrapper {
	public abstract class Container : Widget {
		static Container ()
		{
			RegisterChildItems (typeof (Stetic.Wrapper.Container),
					    new ItemGroup[0]);
		}

		protected Container (IStetic stetic, Gtk.Container container) : base (stetic, container)
		{
			container.Removed += SiteRemoved;
		}

		public static new Container Lookup (GLib.Object obj)
		{
			return Object.Lookup (obj) as Stetic.Wrapper.Container;
		}

		static Hashtable childGroups = new Hashtable ();

		protected static void RegisterChildItems (Type t, params ItemGroup[] items)
		{
			childGroups[t] = items;
		}

		public ItemGroup[] ChildItemGroups {
			get {
				ItemGroup[] items = (ItemGroup[])childGroups[GetType ()];
				if (items == null) {
					for (Type t = GetType ().BaseType; t != null; t = t.BaseType) {
						items = (ItemGroup[])childGroups[t];
						if (items != null) {
							childGroups[GetType ()] = items;
							break;
						}
					}
				}
				return items;
			}
		}

		public abstract bool HExpandable { get; }
		public abstract bool VExpandable { get; }

		public delegate void ContentsChangedHandler (Container container);
		public event ContentsChangedHandler ContentsChanged;

		protected void EmitContentsChanged ()
		{
			if (ContentsChanged != null)
				ContentsChanged (this);
		}

		protected WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			return site;
		}

		protected WidgetSite CreateWidgetSite (int emptyWidth, int emptyHeight)
		{
			WidgetSite site = stetic.CreateWidgetSite (emptyWidth, emptyHeight);
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			return site;
		}

		protected virtual void SiteOccupancyChanged (WidgetSite site) {
			EmitContentsChanged ();
		}

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;

			if (site != null)
				site.OccupancyChanged -= SiteOccupancyChanged;
		}

		public IEnumerable Sites {
			get {
				return ((Gtk.Container)Wrapped).Children;
			}
		}

	}
}

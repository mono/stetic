using System;
using System.Collections;
using System.Reflection;

namespace Stetic.Wrapper {
	public abstract class Container : Widget {

		public static new Type WrappedType = typeof (Gtk.Container);

		static Hashtable childTypes = new Hashtable ();

		static new void Register (Type type)
		{
			// Check if the type declares a
			// Stetic.Wrapper.Container.ContainerChild subtype
			Type childType = typeof (Stetic.Wrapper.Container.ContainerChild);
			foreach (Type ct in type.GetNestedTypes (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
				if (ct.IsSubclassOf (childType)) {
					childTypes[type] = ct;
					Stetic.ObjectWrapper.Register (ct);
				}
			}
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.Container)Wrapped).Removed += SiteRemoved;
		}

		public static new Container Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Container;
		}

		public static Stetic.Wrapper.Container.ContainerChild ChildWrapper (Stetic.IWidgetSite site) {
			Gtk.Widget sitew = site as Gtk.Widget;
			if (sitew == null || site.ParentSite == null)
				return null;

			Gtk.Container parent = site.ParentSite.Contents as Gtk.Container;
			if (parent == null)
				return null;

			Stetic.Wrapper.Container pwrap = Stetic.Wrapper.Container.Lookup (parent);
			if (pwrap == null)
				return null;

			Type ct = null;
			for (Type t = pwrap.GetType (); t != null; t = t.BaseType) {
				ct = childTypes[t] as Type;
				if (ct != null)
					break;
			}
			if (ct == null)
				return null;

			Gtk.Container.ContainerChild cc = parent[sitew];
			return Stetic.ObjectWrapper.Create (pwrap.stetic, ct, cc) as ContainerChild;
		}

		public delegate void ContentsChangedHandler (Container container);
		public event ContentsChangedHandler ContentsChanged;

		protected void EmitContentsChanged ()
		{
			if (ContentsChanged != null)
				ContentsChanged (this);
		}

		protected override WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = base.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			return site;
		}

		public WidgetSite AddSite ()
		{
			return AddSite (null);
		}

		public virtual WidgetSite AddSite (Gtk.Widget child)
		{
			WidgetSite site = CreateWidgetSite ();
			if (child != null) {
				site.Add (child);
			}
			((Gtk.Container)Wrapped).Add (site);
			return site;
		}

		protected virtual void SiteOccupancyChanged (WidgetSite site) {
			EmitContentsChanged ();
		}

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;

			if (site != null) {
				site.OccupancyChanged -= SiteOccupancyChanged;
			}
		}

		protected virtual void SiteRemoved (WidgetSite site)
		{
			;
		}

		public IEnumerable Sites {
			get {
				return ((Gtk.Container)Wrapped).Children;
			}
		}

		public class ContainerChild : Stetic.ObjectWrapper {

			public static new Type WrappedType = typeof (Gtk.Container.ContainerChild);

			static void Register ()
			{
				// FIXME?
			}

			protected override void Wrap (object obj, bool initialized)
			{
				base.Wrap (obj, initialized);
				((Gtk.Container.ContainerChild)Wrapped).Child.ChildNotified += ChildNotifyHandler;

				// FIXME; arrange for wrapper disposal?
			}

			public override void Dispose ()
			{
				((Gtk.Container.ContainerChild)Wrapped).Child.ChildNotified -= ChildNotifyHandler;
				base.Dispose ();
			}

			protected virtual void ChildNotifyHandler (object obj, Gtk.ChildNotifiedArgs args)
			{
				ParamSpec pspec = ParamSpec.Wrap (args.Pspec);
				EmitNotify (pspec.Name);
			}

			protected Stetic.Wrapper.Container ParentWrapper {
				get {
					return Stetic.Wrapper.Container.Lookup (((Gtk.Container.ContainerChild)Wrapped).Parent);
				}
			}
		}
	}
}

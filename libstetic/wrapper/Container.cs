using System;
using System.Collections;
using System.Reflection;

namespace Stetic.Wrapper {
	public abstract class Container : Widget {

		public static new Type WrappedType = typeof (Gtk.Container);

		static Hashtable childTypes = new Hashtable ();

		static new void Register (Type type)
		{
			// Check if the type or one of its ancestors declares a
			// Stetic.Wrapper.Container.ContainerChild subtype
			Type childType = typeof (Stetic.Wrapper.Container.ContainerChild);

			do {
				foreach (Type ct in type.GetNestedTypes (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					if (ct.IsSubclassOf (childType)) {
						if (!childTypes.ContainsValue (ct))
							Stetic.ObjectWrapper.Register (ct);
						childTypes[type] = ct;
						return;
					}
				}
				type = type.BaseType;
			} while (type != typeof (Stetic.Wrapper.Container));
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

		protected Set AutoSize = new Set ();

		protected override WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = base.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			AutoSize[site] = true;
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
			if (!site.Occupied)
				AutoSize[site] = true;
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
			AutoSize[site] = false;
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
				cc.Child.ChildNotified += ChildNotifyHandler;

				// FIXME; arrange for wrapper disposal?
			}

			public override void Dispose ()
			{
				cc.Child.ChildNotified -= ChildNotifyHandler;
				base.Dispose ();
			}

			protected virtual void ChildNotifyHandler (object obj, Gtk.ChildNotifiedArgs args)
			{
				ParamSpec pspec = ParamSpec.Wrap (args.Pspec);
				EmitNotify (pspec.Name);
			}

			Gtk.Container.ContainerChild cc {
				get {
					return (Gtk.Container.ContainerChild)Wrapped;
				}
			}

			protected Stetic.Wrapper.Container ParentWrapper {
				get {
					return Stetic.Wrapper.Container.Lookup (cc.Parent);
				}
			}

			[Description ("Auto Size", "If set, the other packing properties for this cell will be automatically adjusted as other widgets are added to and removed from the container")]
			public bool AutoSize {
				get {
					return ParentWrapper.AutoSize[cc.Child];
				}
				set {
					ParentWrapper.AutoSize[cc.Child] = value;
					EmitNotify ("AutoSize");
				}
			}
		}
	}
}

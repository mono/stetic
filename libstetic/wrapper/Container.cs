using System;
using System.Collections;
using System.Reflection;

namespace Stetic.Wrapper {
	public abstract class Container : Widget {
		static Container ()
		{
			RegisterWrapper (typeof (Stetic.Wrapper.Container),
					 new ItemGroup[0]);
		}

		public static new Container Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Container;
		}

		static Hashtable childTypes = new Hashtable ();

		protected new static void RegisterWrapper (Type t, params ItemGroup[] items)
		{
			Stetic.ObjectWrapper.RegisterWrapper (t, items);

			// Check if it declares a Stetic.Wrapper.Container.ContainerChild subtype
			Type childType = typeof (Stetic.Wrapper.Container.ContainerChild);
			foreach (Type ct in t.GetNestedTypes (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
				if (ct.IsSubclassOf (childType)) {
					childType = ct;
					break;
				}
			}

			// Find the ContainerChild type's constructor
			foreach (ConstructorInfo ctor in childType.GetConstructors ()) {
				ParameterInfo[] parms = ctor.GetParameters ();
				if (parms.Length == 2 &&
				    parms[0].ParameterType == typeof (Stetic.IStetic) &&
				    (parms[1].ParameterType == typeof (Gtk.Container.ContainerChild) ||
				     parms[1].ParameterType.IsSubclassOf (typeof (Gtk.Container.ContainerChild)))) {
					childTypes[t] = ctor;
					break;
				}
			}
			if (childTypes[t] == null)
				throw new ArgumentException ("No suitable constructor for " + childType.FullName);
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

			ConstructorInfo ctor;
			for (Type t = pwrap.GetType (); t != null; t = t.BaseType) {
				ctor = childTypes[t] as ConstructorInfo;
				if (ctor != null)
					break;
			}
			if (ctor == null)
				return null;

			Gtk.Container.ContainerChild cc = parent[sitew];
			return ctor.Invoke (new object[] { pwrap.stetic, cc }) as ContainerChild;
		}

		protected Container (IStetic stetic, Gtk.Container container) : base (stetic, container)
		{
			container.Removed += SiteRemoved;
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

		protected virtual void SiteOccupancyChanged (WidgetSite site) {
			EmitContentsChanged ();
		}

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;

			site.OccupancyChanged -= SiteOccupancyChanged;
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

			static ContainerChild ()
			{
				RegisterWrapper (typeof (Stetic.Wrapper.Container.ContainerChild),
						 new ItemGroup[0]);
			}

			public ContainerChild (IStetic stetic, Gtk.Container.ContainerChild cc) : base (stetic, cc)
			{
				cc.Child.ChildNotified += ChildNotifyHandler;

				// FIXME; arrange for wrapper disposal?
			}

			public override void Dispose ()
			{
				((Gtk.Container.ContainerChild)Wrapped).Parent.ChildNotified -= ChildNotifyHandler;
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

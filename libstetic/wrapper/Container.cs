using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

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

		Gtk.Container container {
			get {
				return (Gtk.Container)Wrapped;
			}
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			container.Removed += SiteRemoved;
		}

		public virtual Widget GladeImportChild (string className, string id,
							ArrayList propNames, ArrayList propVals,
							ArrayList packingNames, ArrayList packingVals)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, propNames, propVals);
			if (wrapper == null)
				return null;

			WidgetSite site = AddPlaceholder ();
			AutoSize[site] = false;
			site.Add ((Gtk.Widget)wrapper.Wrapped);

			GladeUtils.SetPacking (container, site, packingNames, packingVals);
			return (Widget)wrapper;
		}

		public virtual WidgetSite AddPlaceholder ()
		{
			WidgetSite site = CreateWidgetSite ();
			container.Add (site);
			return site;
		}

		protected virtual Gtk.Widget FindInternalChild (string childName)
		{
			// The fact that this tends to work does not mean it is not an
			// awful kludge

			Type type = container.GetType ();
			PropertyInfo pinfo = type.GetProperty (childName.Replace ("_", ""), BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
			if (pinfo != null &&
			    (pinfo.PropertyType == typeof (Gtk.Widget) ||
			     pinfo.PropertyType.IsSubclassOf (typeof (Gtk.Widget))))
				return pinfo.GetValue (container, null) as Gtk.Widget;

			if (container.Parent != null) {
				type = container.Parent.GetType ();
				pinfo = type.GetProperty (childName.Replace ("_", ""), BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
				if (pinfo != null &&
				    (pinfo.PropertyType == typeof (Gtk.Widget) ||
				     pinfo.PropertyType.IsSubclassOf (typeof (Gtk.Widget))))
					return pinfo.GetValue (container.Parent, null) as Gtk.Widget;
			}

			Console.WriteLine ("Could not find internal child {0} of {1}",
					   childName, container);
			return null;
		}

		public virtual Widget GladeSetInternalChild (string childName, string className, string id,
							     ArrayList propNames, ArrayList propVals)
		{
			Gtk.Widget widget = FindInternalChild (childName);
			if (widget == null)
				return null;

			widget.Name = id;
			GladeUtils.SetProps (widget, propNames, propVals);

			return (Widget) Stetic.ObjectWrapper.Create (stetic, className, widget);
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
				return container.Children;
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

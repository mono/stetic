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

		protected virtual void Sync ()
		{
			;
		}

		Gtk.Container container {
			get {
				return (Gtk.Container)Wrapped;
			}
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			container.Removed += SiteRemoved;
		}

		public virtual Widget GladeImportChild (string className, string id,
							Hashtable props, Hashtable childprops)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
			Gtk.Widget child = (Gtk.Widget)wrapper.Wrapped;

			if (container.ChildType () == Gtk.Widget.GType) {
				child = CreateWidgetSite (child);
				AutoSize[child] = false;
			}
			container.Add (child);

			GladeUtils.SetPacking (container, child, childprops);
			return (Widget)wrapper;
		}

		public virtual void GladeExportChild (Widget wrapper, out string className,
						      out string internalId, out string id,
						      out Hashtable props,
						      out Hashtable childprops)
		{
			internalId = null;
			childprops = null;
			wrapper.GladeExport (out className, out id, out props);

			Gtk.Widget child = wrapper.Wrapped as Gtk.Widget;
			while (child != null && !(child is WidgetSite))
				child = child.Parent;

			WidgetSite site = child as WidgetSite;
			if (site != null) {
				if (site.InternalChildId != null)
					internalId = site.InternalChildId;
				else {
					ObjectWrapper childwrapper = ChildWrapper (site);
					if (childwrapper != null)
						GladeUtils.GetProps (childwrapper, out childprops);
				}
			}
		}

		public virtual Placeholder AddPlaceholder ()
		{
			Placeholder ph = CreatePlaceholder ();
			container.Add (ph);
			return ph;
		}

		public virtual WidgetSite AddWidgetSite (Gtk.Widget child)
		{
			WidgetSite site = CreateWidgetSite (child);
			container.Add (site);
			return site;
		}

		Gtk.Widget FindInternalChild (string childId)
		{
			Container ancestor = this;
			while (ancestor != null) {
				foreach (WidgetSite site in ancestor.Sites) {
					if (site.InternalChildId == childId)
						return site.Contents;
				}
				ancestor = Lookup (ancestor.container.Parent);
			}
			return null;
		}

		public virtual Widget GladeSetInternalChild (string childId, string className, string id, Hashtable props)
		{
			Gtk.Widget widget = FindInternalChild (childId);
			if (widget == null)
				throw new GladeException ("Unrecognized internal child name", className, false, "internal-child", childId);

			ObjectWrapper wrapper = Stetic.ObjectWrapper.Create (stetic, className);
			GladeUtils.ImportWidget (stetic, wrapper, widget, id, props);

			return (Widget) wrapper;
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

		protected override WidgetSite CreateWidgetSite (Gtk.Widget w)
		{
			WidgetSite site = base.CreateWidgetSite (w);
			site.ShapeChanged += SiteShapeChanged;
			site.Empty += SiteEmpty;
			return site;
		}

		protected override Placeholder CreatePlaceholder ()
		{
			Placeholder ph = base.CreatePlaceholder ();
			ph.Drop += PlaceholderDrop;
			AutoSize[ph] = true;
			return ph;
		}

		void SiteEmpty (WidgetSite site)
		{
			ReplaceChild (site, CreatePlaceholder ());
			site.Destroy ();
			EmitContentsChanged ();
		}

		void PlaceholderDrop (Placeholder ph, Gtk.Widget dropped)
		{
			WidgetSite site = CreateWidgetSite (dropped);
			ReplaceChild (ph, site);
			ph.Destroy ();
			site.Select ();
			EmitContentsChanged ();
		}

		protected virtual void SiteShapeChanged (WidgetSite site) {
			;
		}

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;
			if (site != null) {
				site.Empty -= SiteEmpty;
				site.ShapeChanged -= SiteShapeChanged;
				SiteRemoved (site);
			}
		}

		protected virtual void SiteRemoved (WidgetSite site)
		{
			AutoSize[site] = false;
			EmitContentsChanged ();
		}

		class SiteEnumerator {
			public ArrayList Sites = new ArrayList ();
			public void Add (Gtk.Widget widget)
			{
				if (widget is WidgetSite)
					Sites.Add (widget);
			}
		}

		public IEnumerable Sites {
			get {
				SiteEnumerator se = new SiteEnumerator ();
				container.Forall (se.Add);
				return se.Sites;
			}
		}

		protected virtual void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			Gtk.Container.ContainerChild cc;
			Hashtable props = new Hashtable ();

			cc = container[oldChild];
			foreach (PropertyInfo pinfo in cc.GetType ().GetProperties ()) {
				if (!pinfo.IsDefined (typeof (Gtk.ChildPropertyAttribute), true))
					continue;
				props[pinfo] = pinfo.GetValue (cc, null);
			}

			container.Remove (oldChild);
			AutoSize[oldChild] = false;
			AutoSize[newChild] = true;
			container.Add (newChild);

			cc = container[newChild];
			foreach (PropertyInfo pinfo in props.Keys)
				pinfo.SetValue (cc, props[pinfo], null);

			Sync ();
		}

		public class ContainerChild : Stetic.ObjectWrapper {

			public static new Type WrappedType = typeof (Gtk.Container.ContainerChild);

			static void Register ()
			{
				// FIXME?
			}

			public override void Wrap (object obj, bool initialized)
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
				ParamSpec pspec = new ParamSpec (args.Pspec);
				EmitNotify (pspec.Name);
			}

			protected override void EmitNotify (string propertyName)
			{
				base.EmitNotify (propertyName);
				ParentWrapper.Sync ();
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

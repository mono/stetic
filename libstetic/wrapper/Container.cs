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

		int freeze;
		protected void Freeze ()
		{
			freeze++;
		}

		protected void Thaw ()
		{
			if (--freeze == 0)
				Sync ();
		}

		protected virtual void DoSync ()
		{
			;
		}

		protected void Sync ()
		{
			if (freeze > 0)
				return;
			freeze = 1;
			DoSync ();
			freeze = 0;
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

			if (InternalChildId != null)
				internalId = InternalChildId;
			else {
				ObjectWrapper childwrapper = ChildWrapper (wrapper);
				if (childwrapper != null)
					GladeUtils.GetProps (childwrapper, out childprops);
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
				foreach (Gtk.Widget w in ancestor.container.Children) {
					Widget wrapper = Lookup (w);
					if (wrapper != null && wrapper.InternalChildId == childId)
						return w;
				}
				ancestor = ParentWrapper;
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

		public static Stetic.Wrapper.Container.ContainerChild ChildWrapper (Stetic.Wrapper.Widget wrapper) {
			Stetic.Wrapper.Container parentWrapper = wrapper.ParentWrapper;
			if (parentWrapper == null)
				return null;

			Gtk.Container parent = parentWrapper.Wrapped as Gtk.Container;
			if (parent == null)
				return null;

			Gtk.Widget child = (Gtk.Widget)wrapper.Wrapped;
			while (child != null && child.Parent != parent)
				child = child.Parent;
			if (child == null)
				return null;

			Type ct = null;
			for (Type t = parentWrapper.GetType (); t != null; t = t.BaseType) {
				ct = childTypes[t] as Type;
				if (ct != null)
					break;
			}
			if (ct == null)
				return null;

			Gtk.Container.ContainerChild cc = parent[child];
			return Stetic.ObjectWrapper.Create (parentWrapper.stetic, ct, cc) as ContainerChild;
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
			site.MotionNotifyEvent += SiteMotionNotify;
			DND.SourceSet (site, false);

			Container childWrapper = Lookup (w);
			if (childWrapper != null)
				childWrapper.ContentsChanged += ChildContentsChanged;

			return site;
		}

		void SiteMotionNotify (object obj, Gtk.MotionNotifyEventArgs args)
		{
			WidgetSite site = obj as WidgetSite;

			if (args.Event.Window != site.HandleWindow ||
			    !DND.CanDrag (site, args.Event)) {
				args.RetVal = true;
				return;
			}

			Placeholder ph = CreatePlaceholder ();
			ph.Mimic (site);
			ReplaceChild (site, ph);

			Gtk.Widget dragWidget = site.Child;
			site.Remove (dragWidget);
			site.Destroy ();
			DND.Drag (ph, args.Event, dragWidget);
		}

		protected override Placeholder CreatePlaceholder ()
		{
			Placeholder ph = base.CreatePlaceholder ();
			ph.Drop += PlaceholderDrop;
			ph.DragEnd += PlaceholderDragEnd;
			AutoSize[ph] = true;
			return ph;
		}

		void PlaceholderDrop (Placeholder ph, Gtk.Widget dropped)
		{
			WidgetSite site = CreateWidgetSite (dropped);
			ReplaceChild (ph, site);
			ph.Destroy ();
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (dropped);
			if (wrapper != null)
				wrapper.Select ();
			EmitContentsChanged ();
		}

		void PlaceholderDragEnd (object obj, Gtk.DragEndArgs args)
		{
			Placeholder ph = obj as Placeholder;

			if (DND.DragWidget == null) {
				ph.UnMimic ();
				Sync ();
			} else
				ReplaceChild (ph, CreateWidgetSite (DND.DragWidget));
		}

		protected virtual void ChildContentsChanged (Container child) {
			;
		}

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;
			if (site != null) {
				Container childWrapper = Lookup (site.Child);
				if (childWrapper != null)
					childWrapper.ContentsChanged -= ChildContentsChanged;

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

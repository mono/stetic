using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Stetic.Wrapper {
	public abstract class Container : Widget {

		public static new Type WrappedType = typeof (Gtk.Container);

		static Hashtable childTypes = new Hashtable ();

		internal static new void Register (Type type)
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

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			container.Removed += ChildRemoved;
			container.SizeAllocated += SizeAllocated;
		}

		Gtk.Container container {
			get {
				return (Gtk.Container)Wrapped;
			}
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

		public virtual Widget GladeImportChild (string className, string id,
							Hashtable props, Hashtable childprops)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
			Gtk.Widget child = (Gtk.Widget)wrapper.Wrapped;

			AutoSize[child] = false;
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

			if (wrapper.InternalChildId != null)
				internalId = wrapper.InternalChildId;
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

		public virtual void Add (Gtk.Widget child)
		{
			container.Add (child);
		}

		Widget FindInternalChild (string childId)
		{
			foreach (Gtk.Widget w in container.AllChildren) {
				Widget wrapper = Lookup (w);
				if (wrapper != null && wrapper.InternalChildId == childId)
					return wrapper;
			}
			return null;
		}

		public virtual Widget GladeSetInternalChild (string childId, string className, string id, Hashtable props)
		{
			Widget wrapper = FindInternalChild (childId);
			if (wrapper == null)
				throw new GladeException ("Unrecognized internal child name", className, false, "internal-child", childId);

			GladeUtils.ImportWidget (stetic, wrapper, wrapper.Wrapped, id, props);

			return (Widget) wrapper;
		}

		public static new Container Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Container;
		}


		public static Container LookupParent (Gtk.Widget widget)
		{
			Gtk.Widget parent = widget.Parent;
			Container wrapper = null;
			while ((wrapper == null || wrapper.Unselectable) && parent != null) {
				wrapper = Lookup (parent);
				parent = parent.Parent;
			}
			return wrapper;
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
			if (ParentWrapper != null)
				ParentWrapper.ChildContentsChanged (this);
		}

		protected Set AutoSize = new Set ();

		protected virtual Placeholder CreatePlaceholder ()
		{
			Placeholder ph = new Placeholder ();
			ph.Show ();
			ph.DragDrop += PlaceholderDragDrop;
			ph.ButtonPressEvent += PlaceholderButtonPress;
			AutoSize[ph] = true;
			return ph;
		}

		void PlaceholderButtonPress (object obj, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Type != Gdk.EventType.ButtonPress)
				return;

			Placeholder ph = obj as Placeholder;

			if (args.Event.Button == 1) {
				Select (ph);
				args.RetVal = true;
			} else if (args.Event.Button == 3) {
				stetic.PopupContextMenu (ph);
				args.RetVal = true;
			}
		}

		void PlaceholderDragDrop (object obj, Gtk.DragDropArgs args)
		{
			Placeholder ph = obj as Placeholder;
			Gtk.Widget dropped = DND.Drop (args.Context, args.Time);
			if (dropped == null)
				return;

			ReplaceChild (ph, dropped);
			ph.Destroy ();
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (dropped);
			if (wrapper != null)
				wrapper.Select ();
			EmitContentsChanged ();
			args.RetVal = true;
		}

		protected virtual void ChildContentsChanged (Container child) {
			;
		}

		void ChildRemoved (object obj, Gtk.RemovedArgs args)
		{
			ChildRemoved (args.Widget);
		}

		protected virtual void ChildRemoved (Gtk.Widget w)
		{
			AutoSize[w] = false;
			EmitContentsChanged ();
		}

		public virtual IEnumerable RealChildren {
			get {
				ArrayList children = new ArrayList ();
				foreach (Gtk.Widget widget in container.AllChildren) {
					if (!(widget is Placeholder))
						children.Add (widget);
				}
				return children;
			}
		}

		public virtual IEnumerable GladeChildren {
			get {
				return RealChildren;
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

		Gtk.Widget selection;
		HandleWindow handles;

		public virtual void Select (Stetic.Wrapper.Widget wrapper)
		{
			if (wrapper == null) {
				Select (null, false);
				stetic.Selection = null;
			} else {
				Select (wrapper.Wrapped, (wrapper.InternalChildId == null));
				stetic.Selection = wrapper.Wrapped;
			}
		}

		public virtual void UnSelect (Gtk.Widget widget)
		{
			if (selection == widget)
				Select (null, false);
		}

		public virtual void Select (Placeholder ph)
		{
			Select (ph, false);
			stetic.Selection = ph;
		}

		void Select (Gtk.Widget widget, bool dragHandles)
		{
			if (widget == selection)
				return;
			selection = widget;

			if (handles != null)
				handles.Dispose ();

			// FIXME: if the selection isn't mapped, we should try to force it
			// to be. (Eg, if you select a widget in a hidden window, the window
			// should map. If you select a widget on a non-current notebook
			// page, the notebook should switch pages, etc.)
			if (selection != null && selection.IsDrawable) {
				handles = new HandleWindow (selection, dragHandles);
				handles.Drag += HandleWindowDrag;
			} else 
				handles = null;
		}

		Gtk.Widget dragSource;

		void HandleWindowDrag (Gdk.EventMotion evt)
		{
			Gtk.Widget dragWidget = selection;

			Select ((Stetic.Wrapper.Widget)null);

			dragSource = CreateDragSource (dragWidget);
			DND.Drag (dragSource, evt, dragWidget);
		}

		protected virtual Gtk.Widget CreateDragSource (Gtk.Widget dragWidget)
		{
			Placeholder ph = CreatePlaceholder ();
			Gdk.Rectangle alloc = dragWidget.Allocation;
			ph.SetSizeRequest (alloc.Width, alloc.Height);
			ph.DragEnd += DragEnd;
			ReplaceChild (dragWidget, ph);
			return ph;
		}

		void DragEnd (object obj, Gtk.DragEndArgs args)
		{
			Placeholder ph = obj as Placeholder;
			ph.DragEnd -= DragEnd;

			dragSource = null;
			if (DND.DragWidget == null) {
				ph.SetSizeRequest (-1, -1);
				Sync ();
			} else
				ReplaceChild (ph, DND.Cancel ());
		}

		void SizeAllocated (object obj, Gtk.SizeAllocatedArgs args)
		{
			if (handles != null)
				handles.Shape ();
		}

		public void Delete (Stetic.Wrapper.Widget wrapper)
		{
			if (wrapper.Wrapped == selection)
				Select (null, false);
			ReplaceChild (wrapper.Wrapped, CreatePlaceholder ());
			wrapper.Wrapped.Destroy ();
		}

		protected bool ChildHExpandable (Gtk.Widget child)
		{
			if (child == dragSource)
				child = DND.DragWidget;
			else if (child is Placeholder)
				return true;

			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (child);
			if (wrapper != null)
				return wrapper.HExpandable;
			else
				return false;
		}

		protected bool ChildVExpandable (Gtk.Widget child)
		{
			if (child == dragSource)
				child = DND.DragWidget;
			else if (child is Placeholder)
				return true;

			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (child);
			if (wrapper != null)
				return wrapper.VExpandable;
			else
				return false;
		}

		public class ContainerChild : Stetic.ObjectWrapper {

			public static new Type WrappedType = typeof (Gtk.Container.ContainerChild);

			internal static void Register ()
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

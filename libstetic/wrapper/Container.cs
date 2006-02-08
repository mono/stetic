using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic.Wrapper {
	public class Container : Widget {
	
		int designWidth;
		int designHeight;

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			ClassDescriptor klass = Registry.LookupClass (obj.GetType ());
			foreach (PropertyDescriptor prop in klass.InternalChildren) {
				Gtk.Widget child = prop.GetValue (container) as Gtk.Widget;
				if (child == null)
					continue;
				Widget wrapper = ObjectWrapper.Create (proj, child) as Stetic.Wrapper.Widget;
				wrapper.InternalChildId = prop.GladeName;
				if (child.Name == ((GLib.GType)child.GetType ()).ToString ())
					child.Name = container.Name + "_" + prop.GladeName;
			}

			if (!initialized && container.Children.Length == 0 && AllowPlaceholders)
				AddPlaceholder ();

			container.Removed += ChildRemoved;
			container.SizeAllocated += SizeAllocated;

			if (Wrapped.GetType ().ToString ()[0] == 'H')
				ContainerOrientation = Gtk.Orientation.Horizontal;
			else
				ContainerOrientation = Gtk.Orientation.Vertical;
		}

		Gtk.Container container {
			get {
				return (Gtk.Container)Wrapped;
			}
		}

		protected virtual bool AllowPlaceholders {
			get {
				return true;
			}
		}
		
		public int DesignWidth {
			get { return designWidth; }
			set { designWidth = value; NotifyChanged (); }
		}

		public int DesignHeight {
			get { return designHeight; }
			set { designHeight = value; NotifyChanged (); }
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

		public override void GladeImport (XmlElement elem)
		{
			base.GladeImport (elem);

			foreach (XmlElement child_elem in elem.SelectNodes ("./child")) {
				try {
					if (child_elem.HasAttribute ("internal-child"))
						GladeSetInternalChild (child_elem);
					else if (child_elem["widget"] == null)
						AddPlaceholder ();
					else
						GladeImportChild (child_elem);
				} catch (GladeException ge) {
					Console.Error.WriteLine (ge.Message);
				}
			}

			string ds = elem.GetAttribute ("design-size");
			if (ds.Length > 0) {
				int i = ds.IndexOf (' ');
				DesignWidth = int.Parse (ds.Substring (0, i));
				DesignHeight = int.Parse (ds.Substring (i+1));
			}
			
			Sync ();
		}

		public virtual Widget GladeImportChild (XmlElement child_elem)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (proj, child_elem["widget"]);

			Gtk.Widget child = (Gtk.Widget)wrapper.Wrapped;

			AutoSize[child] = false;
			container.Add (child);

			GladeUtils.SetPacking (ChildWrapper ((Widget)wrapper), child_elem);
			return (Widget)wrapper;
		}

		public virtual Widget GladeSetInternalChild (XmlElement child_elem)
		{
			ClassDescriptor klass = Registry.LookupClass (Wrapped.GetType ());
			string childId = child_elem.GetAttribute ("internal-child");

			foreach (PropertyDescriptor prop in klass.InternalChildren) {
				if (prop.GladeName != childId)
					continue;

				Gtk.Widget child = prop.GetValue (container) as Gtk.Widget;
				Widget wrapper = Widget.Lookup (child);
				if (wrapper != null) {
					wrapper.GladeImport (child_elem["widget"]);
					GladeUtils.SetPacking (ChildWrapper (wrapper), child_elem);
					return (Widget)wrapper;
				}
			}

			throw new GladeException ("Unrecognized internal child name", Wrapped.GetType ().FullName, false, "internal-child", childId);
		}

		public override XmlElement GladeExport (XmlDocument doc)
		{
			XmlElement elem = base.GladeExport (doc);
			XmlElement child_elem;

			foreach (Gtk.Widget child in GladeChildren) {
				Widget wrapper = Widget.Lookup (child);
				if (wrapper != null)
					child_elem = GladeExportChild (wrapper, doc);
				else {
					child_elem = doc.CreateElement ("child");
					child_elem.AppendChild (doc.CreateElement ("placeholder"));
				}
				elem.AppendChild (child_elem);
			}

			if (DesignWidth != 0 || DesignHeight != 0)
				elem.SetAttribute ("design-size", DesignWidth + " " + DesignHeight);
			return elem;
		}

		public virtual XmlElement GladeExportChild (Widget wrapper, XmlDocument doc)
		{
			XmlElement child_elem = doc.CreateElement ("child");
			XmlElement widget_elem = wrapper.GladeExport (doc);
			child_elem.AppendChild (widget_elem);

			if (wrapper.InternalChildId != null)
				child_elem.SetAttribute ("internal-child", wrapper.InternalChildId);
			else {
				ObjectWrapper childwrapper = ChildWrapper (wrapper);
				if (childwrapper != null) {
					XmlElement packing_elem = doc.CreateElement ("packing");
					GladeUtils.GetProps (childwrapper, packing_elem);
					if (packing_elem.HasChildNodes)
						child_elem.AppendChild (packing_elem);
				}
			}

			return child_elem;
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

			Gtk.Container.ContainerChild cc = parent[child];
			return Stetic.ObjectWrapper.Create (parentWrapper.proj, cc) as ContainerChild;
		}

		protected Gtk.Container.ContainerChild ContextChildProps (Gtk.Widget context)
		{
			if (context == container)
				return null;

			do {
				if (context.Parent == container)
					return container[context];
				context = context.Parent;
			} while (context != null);

			return null;
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
			ph.DragDataReceived += PlaceholderDragDataReceived;
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
				proj.PopupContextMenu (ph);
				args.RetVal = true;
			}
		}

		void PlaceholderDrop (Placeholder ph, Stetic.Wrapper.Widget wrapper)
		{
			ReplaceChild (ph, wrapper.Wrapped);
			ph.Destroy ();
			wrapper.Select ();
		}

		void PlaceholderDragDrop (object obj, Gtk.DragDropArgs args)
		{
			Placeholder ph = (Placeholder)obj;
			Widget dropped = DND.Drop (args.Context, ph, args.Time);
			if (dropped != null) {
				PlaceholderDrop (ph, dropped);
				args.RetVal = true;
			}
		}

		void PlaceholderDragDataReceived (object obj, Gtk.DragDataReceivedArgs args)
		{
			Widget dropped = GladeUtils.Paste (proj, args.SelectionData);
			Gtk.Drag.Finish (args.Context, dropped != null,
					 dropped != null, args.Time);
			if (dropped != null)
				PlaceholderDrop ((Placeholder)obj, dropped);
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
				return container.AllChildren;
			}
		}

		public virtual void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
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
			EmitContentsChanged ();
		}

		Gtk.Widget selection;
		HandleWindow handles;

		public virtual void Select (Stetic.Wrapper.Widget wrapper)
		{
			if (wrapper == null) {
				Select (null, false);
				proj.Selection = null;
			} else {
				Select (wrapper.Wrapped, (wrapper.InternalChildId == null));
				proj.Selection = wrapper.Wrapped;
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
			proj.Selection = ph;
		}

		void Select (Gtk.Widget widget, bool dragHandles)
		{
			if (widget == selection)
				return;

			if (selection != null)
				selection.Destroyed -= SelectionDestroyed;
			if (handles != null)
				handles.Dispose ();

			selection = widget;
			if (selection != null)
				selection.Destroyed += SelectionDestroyed;

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

		void SelectionDestroyed (object obj, EventArgs args)
		{
			UnSelect (selection);
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
				if (AllowPlaceholders)
					ph.SetSizeRequest (-1, -1);
				else
					container.Remove (ph);
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
			if (AllowPlaceholders)
				ReplaceChild (wrapper.Wrapped, CreatePlaceholder ());
			else
				container.Remove (wrapper.Wrapped);
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

		// Note that this will be invalid/random for non-H/V-paired classes
		protected Gtk.Orientation ContainerOrientation;

		public override bool HExpandable {
			get {
				if (base.HExpandable)
					return true;

				// A horizontally-oriented container is HExpandable if any
				// child is. A vertically-oriented container is HExpandable
				// if *every* child is.

				foreach (Gtk.Widget w in container) {
					if (ChildHExpandable (w)) {
						if (ContainerOrientation == Gtk.Orientation.Horizontal)
							return true;
					} else if (ContainerOrientation == Gtk.Orientation.Vertical)
						return false;
				}
				return (ContainerOrientation == Gtk.Orientation.Vertical);
			}
		}

		public override bool VExpandable {
			get {
				if (base.VExpandable)
					return true;

				// Opposite of above

				foreach (Gtk.Widget w in container) {
					if (ChildVExpandable (w)) {
						if (ContainerOrientation == Gtk.Orientation.Vertical)
							return true;
					} else if (ContainerOrientation == Gtk.Orientation.Horizontal)
						return false;
				}
				return (ContainerOrientation == Gtk.Orientation.Horizontal);
			}
		}

		public class ContainerChild : Stetic.ObjectWrapper {

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
				ParentWrapper.OnWidgetChanged (new WidgetEventArgs (ParentWrapper));
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

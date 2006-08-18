using System;
using System.CodeDom;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic.Wrapper {
	public class Container : Widget {
	
		int designWidth;
		int designHeight;
		IDesignArea designer;

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			ClassDescriptor klass = this.ClassDescriptor;
			foreach (PropertyDescriptor prop in klass.InternalChildren) {
				Gtk.Widget child = prop.GetValue (container) as Gtk.Widget;
				if (child == null)
					continue;
				Widget wrapper = ObjectWrapper.Create (proj, child) as Stetic.Wrapper.Widget;
				wrapper.InternalChildProperty = prop;
				if (child.Name == ((GLib.GType)child.GetType ()).ToString ())
					child.Name = container.Name + "_" + prop.Name;
			}

			if (!initialized && container.Children.Length == 0 && AllowPlaceholders)
				AddPlaceholder ();

			container.Removed += ChildRemoved;
			container.Added += OnChildAdded;

			if (Wrapped.GetType ().ToString ()[0] == 'H')
				ContainerOrientation = Gtk.Orientation.Horizontal;
			else
				ContainerOrientation = Gtk.Orientation.Vertical;
			
			if (!Loading)
				ValidateChildNames (Wrapped);
		}
		
		void OnChildAdded (object o, Gtk.AddedArgs args)
		{
			// Make sure children's IDs don't conflict with other widgets
			// in the parent container.
			if (!Loading)
				ValidateChildNames ((Gtk.Widget)o);
			if (designer != null) {
				ObjectWrapper w = ObjectWrapper.Lookup (o);
				if (w != null)
					w.OnDesignerAttach (designer);
			}
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
		
		public void IncreaseBorderWidth () 
		{
			container.BorderWidth += 3;
		}

		public void DecreaseBorderWidth () 
		{
			if (container.BorderWidth >= 3)
				container.BorderWidth -= 3;
			else
				container.BorderWidth = 0;
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
			if (freeze > 0 || Loading)
				return;
			freeze = 1;
			DoSync ();
			freeze = 0;
		}

		public override void Read (XmlElement elem, FileFormat format)
		{
			base.Read (elem, format);
			ReadChildren (elem, format);
		}
		
		protected virtual void ReadChildren (XmlElement elem, FileFormat format)
		{
			foreach (XmlElement child_elem in elem.SelectNodes ("./child")) {
				try {
					if (child_elem.HasAttribute ("internal-child"))
						ReadInternalChild (child_elem, format);
					else if (child_elem["widget"] == null)
						AddPlaceholder ();
					else
						ReadChild (child_elem, format);
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

		protected virtual void ReadChild (XmlElement child_elem, FileFormat format)
		{
			ObjectWrapper wrapper = Stetic.ObjectWrapper.Read (proj, child_elem["widget"], format);

			Gtk.Widget child = (Gtk.Widget)wrapper.Wrapped;

			AutoSize[child] = false;
			container.Add (child);

			if (format == FileFormat.Glade)
				GladeUtils.SetPacking (ChildWrapper ((Widget)wrapper), child_elem);
			else
				WidgetUtils.SetPacking (ChildWrapper ((Widget)wrapper), child_elem);
		}

		protected virtual void ReadInternalChild (XmlElement child_elem, FileFormat format)
		{
			TypedClassDescriptor klass = base.ClassDescriptor as TypedClassDescriptor;
			string childId = child_elem.GetAttribute ("internal-child");
			
			foreach (PropertyDescriptor prop in klass.InternalChildren) {
				if (format == FileFormat.Glade && ((TypedPropertyDescriptor)prop).GladeName != childId)
					continue;
				else if (format == FileFormat.Native && prop.Name != childId)
					continue;
				
				Gtk.Widget child = prop.GetValue (container) as Gtk.Widget;
				Widget wrapper = Widget.Lookup (child);
				if (wrapper != null) {
					wrapper.Read (child_elem["widget"], format);
					if (format == FileFormat.Glade)
						GladeUtils.SetPacking (ChildWrapper (wrapper), child_elem);
					else
						WidgetUtils.SetPacking (ChildWrapper (wrapper), child_elem);
					return;
				}
			}
			
			throw new GladeException ("Unrecognized internal child name", Wrapped.GetType ().FullName, false, "internal-child", childId);
		}

		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			XmlElement child_elem;
			
			if (ClassDescriptor.AllowChildren) {
				foreach (Gtk.Widget child in GladeChildren) {
					Widget wrapper = Widget.Lookup (child);
					
					if (wrapper != null) {
						// Iternal children are written later
						if (wrapper.InternalChildProperty != null)
							continue;
						child_elem = WriteChild (wrapper, doc, format);
						if (child_elem != null)
							elem.AppendChild (child_elem);
					} else if (child is Stetic.Placeholder) {
						child_elem = doc.CreateElement ("child");
						child_elem.AppendChild (doc.CreateElement ("placeholder"));
						elem.AppendChild (child_elem);
					}
				}
			}
			
			foreach (PropertyDescriptor prop in this.ClassDescriptor.InternalChildren) {
				Gtk.Widget child = prop.GetValue (Wrapped) as Gtk.Widget;
				if (child == null)
					continue;

				child_elem = doc.CreateElement ("child");
				Widget wrapper = Widget.Lookup (child);
				if (wrapper == null) {
					child_elem.AppendChild (doc.CreateElement ("placeholder"));
					elem.AppendChild (child_elem);
					continue;
				}
				
				string cid = format == FileFormat.Glade ? prop.InternalChildId : prop.Name;
				
				XmlElement widget_elem = wrapper.Write (doc, format);
				child_elem.SetAttribute ("internal-child", cid);
				
				child_elem.AppendChild (widget_elem);
				elem.AppendChild (child_elem);
			}

			if (DesignWidth != 0 || DesignHeight != 0)
				elem.SetAttribute ("design-size", DesignWidth + " " + DesignHeight);
			return elem;
		}

		protected virtual XmlElement WriteChild (Widget wrapper, XmlDocument doc, FileFormat format)
		{
			XmlElement child_elem = doc.CreateElement ("child");
			XmlElement widget_elem = wrapper.Write (doc, format);
			child_elem.AppendChild (widget_elem);

			ObjectWrapper childwrapper = ChildWrapper (wrapper);
			if (childwrapper != null) {
				XmlElement packing_elem = doc.CreateElement ("packing");
				
				if (format == FileFormat.Glade)
					GladeUtils.GetProps (childwrapper, packing_elem);
				else
					WidgetUtils.GetProps (childwrapper, packing_elem);
					
				if (packing_elem.HasChildNodes)
					child_elem.AppendChild (packing_elem);
			}

			return child_elem;
		}
		
		public XmlElement WriteContainerChild (Widget wrapper, XmlDocument doc, FileFormat format)
		{
			return WriteChild (wrapper, doc, format);
		}
		
		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			base.GenerateBuildCode (ctx, varName);
			
			if (ClassDescriptor.AllowChildren) {
				foreach (Gtk.Widget child in GladeChildren) {
					Widget wrapper = Widget.Lookup (child);
					
					if (wrapper != null && wrapper.InternalChildProperty == null)
						// Iternal children are written later
						GenerateChildBuildCode (ctx, varName, wrapper);
				}
			}
			
			foreach (TypedPropertyDescriptor prop in this.ClassDescriptor.InternalChildren) {
				GenerateSetInternalChild (ctx, varName, prop);
			}
		}
		
		protected virtual void GenerateChildBuildCode (GeneratorContext ctx, string parentVar, Widget wrapper)
		{
			ObjectWrapper childwrapper = ChildWrapper (wrapper);
			if (childwrapper != null) {
				ctx.Statements.Add (new CodeCommentStatement ("Container child " + Wrapped.Name + "." + childwrapper.Wrapped.GetType ()));
				string varName = ctx.GenerateNewInstanceCode (wrapper);
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (
					new CodeVariableReferenceExpression (parentVar),
					"Add",
					new CodeVariableReferenceExpression (varName)
				);
				ctx.Statements.Add (invoke);

				GenerateSetPacking (ctx, parentVar, varName, childwrapper);
			}
		}
		
		void GenerateSetInternalChild (GeneratorContext ctx, string parentVar, TypedPropertyDescriptor prop)
		{
			Gtk.Widget child = prop.GetValue (container) as Gtk.Widget;
			Widget cwrapper = Widget.Lookup (child);
			if (cwrapper != null) {
				ctx.Statements.Add (new CodeCommentStatement ("Internal child " + Wrapped.Name + "." + prop.Name));
				string childVar = ctx.NewId ();
				CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (child.GetType(), childVar);
				ctx.Statements.Add (varDec);
				varDec.InitExpression = new CodePropertyReferenceExpression (new CodeVariableReferenceExpression (parentVar), prop.Name);
			
				ctx.GenerateBuildCode (cwrapper, childVar);
				return;
			}
		}
		
		protected void GenerateSetPacking (GeneratorContext ctx, string parentVar, string childVar, ObjectWrapper containerChildWrapper)
		{
			Gtk.Container.ContainerChild cc = containerChildWrapper.Wrapped as Gtk.Container.ContainerChild;
			ClassDescriptor klass = containerChildWrapper.ClassDescriptor;
			
			// Generate a variable that holds the container child
			
			string contChildVar = ctx.NewId ();
			CodeVariableDeclarationStatement varDec = new CodeVariableDeclarationStatement (cc.GetType(), contChildVar);
			varDec.InitExpression = new CodeCastExpression ( 
				cc.GetType (),
				new CodeIndexerExpression (new CodeVariableReferenceExpression (parentVar), new CodeVariableReferenceExpression (childVar))
			);
			
			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression (contChildVar);
			
			// Set the container child properties

			ctx.Statements.Add (varDec);
			int count = ctx.Statements.Count;
			
			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || !prop.IsRuntimeProperty)
						continue;
					GenerateChildPropertySet (ctx, var, klass, prop, cc);
				}
			}
			
			if (ctx.Statements.Count == count) {
				ctx.Statements.Remove (varDec);
			}
		}
		
		protected virtual void GenerateChildPropertySet (GeneratorContext ctx, CodeVariableReferenceExpression var, ClassDescriptor containerChildClass, PropertyDescriptor prop, object child)
		{
			if (containerChildClass.InitializationProperties != null && Array.IndexOf (containerChildClass.InitializationProperties, prop) != -1)
				return;
			
			// Design time
			if (prop.Name == "AutoSize")
				return;
				
			object oval = prop.GetValue (child);
			if (oval == null || (prop.HasDefault && prop.IsDefaultValue (oval)))
				return;
				
			CodePropertyReferenceExpression cprop = new CodePropertyReferenceExpression (var, prop.Name);
			CodeExpression val = ctx.GenerateValue (oval, prop.RuntimePropertyType);
			ctx.Statements.Add (new CodeAssignStatement (cprop, val));
		}
		
		internal protected override void OnDesignerAttach (IDesignArea designer)
		{
			this.designer = designer;
			foreach (Gtk.Widget w in RealChildren) {
				ObjectWrapper wr = ObjectWrapper.Lookup (w);
				if (wr != null)
					wr.OnDesignerAttach (designer);
			}
		}
		
		internal protected override void OnDesignerDetach (IDesignArea designer)
		{
			foreach (Gtk.Widget w in RealChildren) {
				ObjectWrapper wr = ObjectWrapper.Lookup (w);
				if (wr != null)
					wr.OnDesignerDetach (designer);
			}
			this.designer = null;
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
			Container.ContainerChild cwrap = ObjectWrapper.Lookup (cc) as Container.ContainerChild;
			if (cwrap != null)
				return cwrap;
			else
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
			Gtk.Widget w = DND.Drop (args.Context, ph, args.Time);
			Widget dropped = Stetic.Wrapper.Widget.Lookup (w);
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
			if (Loading)
				return;
				
			ObjectWrapper w = ObjectWrapper.Lookup (args.Widget);
			if (w != null) {
				if (w.Loading)
					return;
				if (designer != null)
					w.OnDesignerDetach (designer);
			}
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

		public virtual void Select (Stetic.Wrapper.Widget wrapper)
		{
			if (wrapper == null) {
				Select (null, false);
				proj.Selection = null;
			} else {
				Select (wrapper.Wrapped, (wrapper.InternalChildProperty == null));
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

			Gtk.Window win = GetParentWindow ();
			
			if (selection != null) {
				selection.Destroyed -= SelectionDestroyed;
				HideSelectionBox (selection);
				Widget wr = Widget.Lookup (selection);
				if (wr != null)
					wr.OnUnselected ();
			}
			
			selection = widget;
			if (win != null) {
				if (widget != null && widget.CanFocus)
					win.Focus = widget;
				else if (widget != null) {
					// Remove the focus from the window. In this way we ensure
					// that the current selected widget will lose the focus,
					// even if the new selection is not focusable.
					Gtk.Widget w = widget.Parent;
					while (w != null && !w.CanFocus)
						w = w.Parent;
					win.Focus = w;
				} else
					win.Focus = null;
			}
				
			if (selection != null) {
				selection.Destroyed += SelectionDestroyed;

				// FIXME: if the selection isn't mapped, we should try to force it
				// to be. (Eg, if you select a widget in a hidden window, the window
				// should map. If you select a widget on a non-current notebook
				// page, the notebook should switch pages, etc.)
				if (selection.IsDrawable && Visible) {
					ShowSelectionBox (selection, dragHandles);
				}
				
				Widget wr = Widget.Lookup (selection);
				if (wr != null)
					wr.OnSelected ();
			}
		}
		
		void ShowSelectionBox (Gtk.Widget widget, bool dragHandles)
		{
			HideSelectionBox (selection);

			IDesignArea designArea = GetDesignArea (widget);
			if (designArea != null) {
				IObjectSelection sel = designArea.SetSelection (widget, widget);
				sel.Drag += HandleWindowDrag;
				return;
			}
		}
		
		void HideSelectionBox (Gtk.Widget widget)
		{
			if (widget != null) {
				IDesignArea designArea = GetDesignArea (widget);
				if (designArea != null)
					designArea.ResetSelection (widget);
			}
		}
		
		Gtk.Window GetParentWindow ()
		{
			Gtk.Container cc = Wrapped as Gtk.Container;
			while (cc.Parent != null)
				cc = cc.Parent as Gtk.Container;
			return cc as Gtk.Window;
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

		public virtual void Delete (Stetic.Wrapper.Widget wrapper)
		{
			if (AllowPlaceholders)
				ReplaceChild (wrapper.Wrapped, CreatePlaceholder ());
			else
				container.Remove (wrapper.Wrapped);
			wrapper.Wrapped.Destroy ();
		}

		public virtual void Delete (Stetic.Placeholder ph)
		{
			if (AllowPlaceholders) {
				// Don't allow deleting the only placeholder of a top level container
				if (IsTopLevel && container.Children.Length == 1)
					return;
				container.Remove (ph);
				ph.Destroy ();
				// If there aren't more placeholders in this container, just delete the container
				if (container.Children.Length == 0)
					Delete ();
			}
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
		
		void ValidateChildNames (Gtk.Widget newWidget)
		{
			// newWidget is the widget which triggered the name check.
			// It will be the last widget to check, so if there are
			// name conflicts, the name to change to avoid the conflict
			// will be the name of that widget.
			
			if (!IsTopLevel) {
				ParentWrapper.ValidateChildNames (newWidget);
				return;
			}
				
			Hashtable names = new Hashtable ();
			
			// Validate all names excluding the new widget
			ValidateChildName (names, container, newWidget);
			
			if (newWidget != null) {
				// Now validate names in the new widget.
				ValidateChildName (names, newWidget, null);
			}
		}

		void ValidateChildName (Hashtable names, Gtk.Widget w, Gtk.Widget newWidget)
		{
			if (w == newWidget)
				return;

			if (names.Contains (w.Name)) {
				// There is a widget with the same name. If the widget
				// has a numeric suffix, just increase it.
				string name; int idx;
				WidgetUtils.ParseWidgetName (w.Name, out name, out idx);
				
				string compName = idx != 0 ? name + idx : name;
				while (names.Contains (compName)) {
					idx++;
					compName = name + idx;
				}
				w.Name = compName;
			}
			
			names [w.Name] = w;
			
			if (w is Gtk.Container) {
				foreach (Gtk.Widget cw in ((Gtk.Container)w).AllChildren)
					ValidateChildName (names, cw, newWidget);
			}
		}
		
		internal string GetValidWidgetName (Gtk.Widget widget)
		{
			// Get a valid name for a widget (a name that doesn't
			// exist in the parent container.

			if (!IsTopLevel)
				return ParentWrapper.GetValidWidgetName (widget);

			string name;
			int idx;

			WidgetUtils.ParseWidgetName (widget.Name, out name, out idx);
			
			string compName = idx != 0 ? name + idx : name;
			
			Gtk.Widget fw = FindWidget (compName, widget);
			while (fw != null) {
				idx++;
				compName = name + idx;
				fw = FindWidget (compName, widget);
			}
			
			return compName;
		}
		
		Gtk.Widget FindWidget (string name, Gtk.Widget skipwidget)
		{
			if (Wrapped != skipwidget && Wrapped.Name == name)
				return Wrapped;
			else
				return FindWidget ((Gtk.Container)Wrapped, name, skipwidget);
		}
		
		Gtk.Widget FindWidget (Gtk.Container parent, string name, Gtk.Widget skipwidget)
		{
			foreach (Gtk.Widget w in parent.AllChildren) {
				if (w.Name == name && w != skipwidget)
					return w;
				if (w is Gtk.Container) {
					Gtk.Widget res = FindWidget ((Gtk.Container)w, name, skipwidget);
					if (res != null)
						return res;
				}
			}
			return null;
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
				cc.Child.ParentSet += OnParentSet;
			}
			
			[GLib.ConnectBefore]
			void OnParentSet (object ob, Gtk.ParentSetArgs args)
			{
				// Dispose the wrapper if the child is removed from the parent
				Gtk.Widget w = (Gtk.Widget)ob;
				if (w.Parent == null) {
					Dispose ();
					w.ParentSet -= OnParentSet;
				}
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
				ParentWrapper.NotifyChanged ();
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

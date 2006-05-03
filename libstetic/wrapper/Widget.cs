using System;
using System.Collections;
using System.Xml;
using System.CodeDom;

namespace Stetic.Wrapper {

	public class Widget : Object {
	
		string oldName;
		bool settingFocus;
		bool hexpandable, vexpandable;

		bool window_visible = true;
		bool hasDefault;
		Gdk.EventMask events;
		ActionGroup actionGroup;
		
		public bool Unselectable;
		
		public Widget ()
		{
		}
	
		// Fired when the name of the widget changes.
		public event WidgetNameChangedHandler NameChanged;
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			
			oldName = ((Gtk.Widget)obj).Name;
			events = Wrapped.Events;

			if (!(Wrapped is Gtk.Window))
				Wrapped.ShowAll ();
			
			Wrapped.PopupMenu += PopupMenu;
			Wrapped.FocusInEvent += OnFocusIn;
			InterceptClicks (Wrapped);

			hexpandable = this.ClassDescriptor.HExpandable;
			vexpandable = this.ClassDescriptor.VExpandable;
			
			if (ParentWrapper != null) {
				// Make sure the widget's name is not already being used.
				string nn = ParentWrapper.GetValidWidgetName (Wrapped);
				if (nn != Wrapped.Name)
					Wrapped.Name = nn;
			}
		}
		
		void OnFocusIn (object s, Gtk.FocusInEventArgs a)
		{
			if (settingFocus)
				return;

			if (!Unselectable)
				Select ();
			else if (ParentWrapper != null)
				ParentWrapper.Select ();
		}
		
		void InterceptClicks (Gtk.Widget widget)
		{
			widget.Events |= Gdk.EventMask.ButtonPressMask;
			widget.WidgetEvent += WidgetEvent;

			Gtk.Container container = widget as Gtk.Container;
			if (container != null) {
				foreach (Gtk.Widget child in container.AllChildren) {
					if (Lookup (child) == null)
						InterceptClicks (child);
				}
			}
		}
		
		public new Gtk.Widget Wrapped {
			get {
				return (Gtk.Widget)base.Wrapped;
			}
		}

		public Stetic.Wrapper.Container ParentWrapper {
			get {
				return Container.LookupParent (Wrapped);
			}
		}
		
		public bool IsTopLevel {
			get { return Wrapped.Parent == null || Widget.Lookup (Wrapped.Parent) == null; }
		}

		public Container GetTopLevel ()
		{
			Widget c = this;
			while (!c.IsTopLevel)
				c = c.ParentWrapper;
			return c as Container;
		}
		
		public ActionGroup LocalActionGroup {
			get {
				if (IsTopLevel) {
#if ACTIONS
					if (actionGroup == null)
						actionGroup = new ActionGroup ();
#endif
					return actionGroup;
				} else {
					return ParentWrapper.LocalActionGroup;
				}
			}
		}
		
		[GLib.ConnectBefore]
		void WidgetEvent (object obj, Gtk.WidgetEventArgs args)
		{
			if (args.Event.Type == Gdk.EventType.ButtonPress)
				args.RetVal = HandleClick ((Gdk.EventButton)args.Event);
		}

		internal bool HandleClick (Gdk.EventButton evb)
		{
			int x = (int)evb.X, y = (int)evb.Y;
			int erx, ery, wrx, wry;

			// Translate from event window to widget window coords
			evb.Window.GetOrigin (out erx, out ery);
			Wrapped.GdkWindow.GetOrigin (out wrx, out wry);
			x += erx - wrx;
			y += ery - wry;

			Widget wrapper = FindWrapper (Wrapped, x, y);
			if (wrapper == null)
				return false;

			if (wrapper.Wrapped != proj.Selection) {
				wrapper.Select ();
				return true;
			} else if (evb.Button == 3) {
				proj.PopupContextMenu (wrapper);
				return true;
			} else
				return false;
		}

		Widget FindWrapper (Gtk.Widget top, int x, int y)
		{
			Widget wrapper;

			Gtk.Container container = top as Gtk.Container;
			if (container != null) {
				foreach (Gtk.Widget child in container.AllChildren) {
					if (!child.IsDrawable)
						continue;

					Gdk.Rectangle alloc = child.Allocation;
					if (alloc.Contains (x, y)) {
						if (child.GdkWindow == top.GdkWindow)
							wrapper = FindWrapper (child, x, y);
						else
							wrapper = FindWrapper (child, x - alloc.X, y - alloc.Y);
						if (wrapper != null)
							return wrapper;
					}
				}
			}

			wrapper = Lookup (top);
			if (wrapper == null || wrapper.Unselectable)
				return null;
			return wrapper;
		}

		void PopupMenu (object obj, EventArgs args)
		{
			proj.PopupContextMenu (this);
		}

		public void Select ()
		{
			// Select() will bring the focus to this widget.
			// This flags avoids calling Select() again
			// when the focusIn event is received.
			settingFocus = true;
			
			if (ParentWrapper != null)
				ParentWrapper.Select (this);
			else if (this is Stetic.Wrapper.Container)
				((Container)this).Select (this);
				
			settingFocus = false;
		}
		
		internal protected virtual void OnSelected ()
		{
		}

		internal protected virtual void OnUnselected ()
		{
		}

		public void Delete ()
		{
			if (ParentWrapper != null)
				ParentWrapper.Delete (this);
			else
				Wrapped.Destroy ();
		}

		public override void Read (XmlElement elem, FileFormat format)
		{
			if (format == FileFormat.Native) {
				XmlElement groupElem = elem ["action-group"];
				if (groupElem != null) {
					actionGroup = new ActionGroup ();
					actionGroup.Read (Project, groupElem);
				}
				WidgetUtils.Read (this, elem);
			}
			else if (format == FileFormat.Glade)
				GladeUtils.ImportWidget (this, elem);
		}

		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			if (format == FileFormat.Native) {
				XmlElement elem = WidgetUtils.Write (this, doc);
				if (actionGroup != null)
					elem.InsertBefore (actionGroup.Write (doc, format), elem.FirstChild);
				return elem;
			}
			else {
				XmlElement elem = GladeUtils.ExportWidget (this, doc);
				GladeUtils.ExtractProperty (elem, "name", "");
				return elem;
			}
		}
		
		internal protected virtual void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			CodeVariableReferenceExpression var = new CodeVariableReferenceExpression (varName);
			
			foreach (ItemGroup group in base.ClassDescriptor.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || !prop.IsRuntimeProperty)
						continue;
					GeneratePropertySet (ctx, var, prop);
				}
			}
		}
		
		internal protected virtual void GeneratePostBuildCode (GeneratorContext ctx, string varName)
		{
			PropertyDescriptor prop = ClassDescriptor ["Visible"] as PropertyDescriptor;
			if (prop != null && prop.PropertyType == typeof(bool) && (bool) prop.GetValue (Wrapped)) {
				if ((bool) prop.GetValue (Wrapped)) {
					ctx.Statements.Add (
						new CodeMethodInvokeExpression (
							new CodeVariableReferenceExpression (varName), 
							"Show"
						)
					);
				}
			}
		}
		
		internal protected virtual CodeExpression GenerateWidgetCreation (GeneratorContext ctx)
		{
			if (ClassDescriptor.InitializationProperties != null) {
				CodeExpression[] paramters = new CodeExpression [ClassDescriptor.InitializationProperties.Length];
				for (int n=0; n < paramters.Length; n++) {
					PropertyDescriptor prop = ClassDescriptor.InitializationProperties [n];
					paramters [n] = ctx.GenerateValue (prop.GetValue (Wrapped), prop.RuntimePropertyType);
				}
				return new CodeObjectCreateExpression (ClassDescriptor.WrappedTypeName, paramters);
			} else
				return new CodeObjectCreateExpression (ClassDescriptor.WrappedTypeName);
		}
		
		protected virtual void GeneratePropertySet (GeneratorContext ctx, CodeVariableReferenceExpression var, PropertyDescriptor prop)
		{
			if (ClassDescriptor.InitializationProperties != null && Array.IndexOf (ClassDescriptor.InitializationProperties, prop) != -1)
				return;
			
			object oval = prop.GetValue (Wrapped);
			if (oval == null || (prop.HasDefault && prop.IsDefaultValue (oval)))
				return;

			CodeExpression val = ctx.GenerateValue (oval, prop.RuntimePropertyType);
			CodeExpression cprop;
			
			TypedPropertyDescriptor tprop = prop as TypedPropertyDescriptor;
			if (tprop == null || tprop.GladeProperty == prop) {
				cprop = new CodePropertyReferenceExpression (var, prop.Name);
			} else {
				cprop = new CodePropertyReferenceExpression (var, tprop.GladeProperty.Name);
				cprop = new CodePropertyReferenceExpression (cprop, prop.Name);
			}
			
			// The Visible property is set after everything is built
			
			if (prop.Name != "Visible")
				ctx.Statements.Add (new CodeAssignStatement (cprop, val));
		}

		public static new Widget Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Widget;
		}

		PropertyDescriptor internalChildProperty;
		public PropertyDescriptor InternalChildProperty {
			get {
				return internalChildProperty;
			}
			set {
				internalChildProperty = value;
			}
		}

		public virtual void Drop (Gtk.Widget widget, object faultId)
		{
			widget.Destroy ();
		}

		public virtual bool HExpandable { get { return hexpandable; } }
		public virtual bool VExpandable { get { return vexpandable; } }

		public bool Visible {
			get {
				return window_visible;
			}
			set {
				window_visible = value;
				EmitNotify ("Visible");
			}
		}

		public bool HasDefault {
			get {
				return hasDefault;
			}
			set {
				hasDefault = value;

				if (Wrapped.Toplevel != null && Wrapped.Toplevel.IsTopLevel)
					Wrapped.HasDefault = hasDefault;
				else
					Wrapped.HierarchyChanged += HierarchyChanged;
			}
		}

		public bool Sensitive {
			get {
				return Wrapped.Sensitive;
			}
			set {
				if (Wrapped.Sensitive == value)
					return;

				Wrapped.Sensitive = value;
				if (Wrapped.Sensitive)
					InsensitiveManager.Remove (this);
				else
					InsensitiveManager.Add (this);
				EmitNotify ("Sensitive");
			}
		}

		public Gdk.EventMask Events {
			get {
				return events;
			}
			set {
				events = value;
				EmitNotify ("Events");
			}
		}

		void HierarchyChanged (object obj, Gtk.HierarchyChangedArgs args)
		{
			if (Wrapped.Toplevel != null && Wrapped.Toplevel.IsTopLevel) {
				Wrapped.HasDefault = hasDefault;
				Wrapped.HierarchyChanged -= HierarchyChanged;
			}
		}

		public string Tooltip {
			get {
				return proj.Tooltips[Wrapped];
			}
			set {
				proj.Tooltips[Wrapped] = value;
			}
		}

		public override string ToString ()
		{
			if (Wrapped.Name != null)
				return "[" + Wrapped.GetType ().Name + " '" + Wrapped.Name + "' " + Wrapped.GetHashCode ().ToString () + "]";
			else
				return "[" + Wrapped.GetType ().Name + " " + Wrapped.GetHashCode ().ToString () + "]";
		}
		
		public IDesignArea GetDesignArea ()
		{
			return GetDesignArea (Wrapped);
		}
		
		protected IDesignArea GetDesignArea (Gtk.Widget w)
		{
			while (w != null && !(w is IDesignArea))
				w = w.Parent;
			return w as IDesignArea;
		}
		
		protected override void EmitNotify (string propertyName)
		{
			base.EmitNotify (propertyName);
			
			// Don't notify parent change for top level widgets.
			if (propertyName == "parent" || propertyName == "has-focus" || 
				propertyName == "has-toplevel-focus" || propertyName == "is-active" ||
				propertyName == "is-focus" || propertyName == "style" || 
				propertyName == "Visible" || propertyName == "scroll-offset")
				return;
			
			if (propertyName == "Name") {
				if (Wrapped.Name != oldName) {
					if (ParentWrapper != null) {
						string nn = ParentWrapper.GetValidWidgetName (Wrapped);
						if (nn != Wrapped.Name) {
							Wrapped.Name = nn;
							return;
						}
					}
					
					string on = oldName;
					oldName = Wrapped.Name;
					OnNameChanged (new WidgetNameChangedArgs (this, on, Wrapped.Name));
				}
			}
			else {
//				Console.WriteLine ("PROP: " + propertyName);
				NotifyChanged ();
			}
		}
		
		protected virtual void OnNameChanged (WidgetNameChangedArgs args)
		{
			NotifyChanged ();
			if (NameChanged != null)
				NameChanged (this, args);
		}
	}

	internal static class InsensitiveManager {

		static Gtk.Invisible invis;
		static Hashtable map;

		static InsensitiveManager ()
		{
			map = new Hashtable ();
			invis = new Gtk.Invisible ();
			invis.ButtonPressEvent += ButtonPress;
		}

		static void ButtonPress (object obj, Gtk.ButtonPressEventArgs args)
		{
			Gtk.Widget widget = (Gtk.Widget)map[args.Event.Window];
			if (widget == null)
				return;

			Widget wrapper = Widget.Lookup (widget);
			args.RetVal = wrapper.HandleClick (args.Event);
		}

		public static void Add (Widget wrapper)
		{
			Gtk.Widget widget = wrapper.Wrapped;

			widget.SizeAllocated += Insensitive_SizeAllocate;
			widget.Realized += Insensitive_Realized;
			widget.Unrealized += Insensitive_Unrealized;
			widget.Mapped += Insensitive_Mapped;
			widget.Unmapped += Insensitive_Unmapped;

			if (widget.IsRealized)
				Insensitive_Realized (widget, EventArgs.Empty);
			if (widget.IsMapped)
				Insensitive_Mapped (widget, EventArgs.Empty);
		}

		public static void Remove (Widget wrapper)
		{
			Gtk.Widget widget = wrapper.Wrapped;
			Gdk.Window win = (Gdk.Window)map[widget];
			if (win != null) {
				map.Remove (widget);
				map.Remove (win);
				win.Destroy ();
			}
			widget.SizeAllocated -= Insensitive_SizeAllocate;
			widget.Realized -= Insensitive_Realized;
			widget.Unrealized -= Insensitive_Unrealized;
			widget.Mapped -= Insensitive_Mapped;
			widget.Unmapped -= Insensitive_Unmapped;
		}

		static void Insensitive_SizeAllocate (object obj, Gtk.SizeAllocatedArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			if (win != null)
				win.MoveResize (args.Allocation);
		}

		static void Insensitive_Realized (object obj, EventArgs args)
		{
			Gtk.Widget widget = (Gtk.Widget)obj;

			Gdk.WindowAttr attributes = new Gdk.WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.Wclass = Gdk.WindowClass.InputOnly;
			attributes.Mask = Gdk.EventMask.ButtonPressMask;

			Gdk.Window win = new Gdk.Window (widget.GdkWindow, attributes, 0);
			win.UserData = invis.Handle;
			win.MoveResize (widget.Allocation);

			map[widget] = win;
			map[win] = widget;
		}

		static void Insensitive_Mapped (object obj, EventArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.Show ();
		}

		static void Insensitive_Unmapped (object obj, EventArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.Hide ();
		}

		static void Insensitive_Unrealized (object obj, EventArgs args)
		{
			Gdk.Window win = (Gdk.Window)map[obj];
			win.Destroy ();
			map.Remove (obj);
			map.Remove (win);
		}
	}
}

using System;
using System.Collections;
using System.Xml;
using System.CodeDom;

namespace Stetic.Wrapper {

	public class Widget : Object {
	
		string oldName;
		string oldMemberName;
		bool settingFocus;
		bool hexpandable, vexpandable;

		bool window_visible = true;
		bool hasDefault;
		Gdk.EventMask events;
		ActionGroupCollection actionGroups;
		string member;
		string tooltip;
		CodeExpression generatedTooltips;
		
		// Name of the generated UIManager
		string uiManagerName;
		// List of groups added to the UIManager
		ArrayList includedActionGroups;
		
		public bool Unselectable;
		
		public Widget ()
		{
		}
	
		// Fired when the name of the widget changes.
		public event WidgetNameChangedHandler NameChanged;
		// Fired when the member name of the widget changes.
		public event WidgetNameChangedHandler MemberNameChanged;
		
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
		
		public string UIManagerName {
			get { return uiManagerName; }
		}
		
		public string MemberName {
			get { return member != null ? member : ""; }
			set { member = value; EmitNotify ("MemberName"); }
		}

		public Container GetTopLevel ()
		{
			Widget c = this;
			while (!c.IsTopLevel)
				c = c.ParentWrapper;
			return c as Container;
		}
		
		public ActionGroupCollection LocalActionGroups {
			get {
				if (IsTopLevel) {
					if (actionGroups == null) {
						actionGroups = new ActionGroupCollection ();
						actionGroups.ActionGroupAdded += OnGroupAdded;
						actionGroups.ActionGroupRemoved += OnGroupRemoved;
						actionGroups.ActionGroupChanged += OnGroupChanged;
					}
					return actionGroups;
				} else {
					return ParentWrapper.LocalActionGroups;
				}
			}
		}
		
		void OnGroupAdded (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded += OnSignalAdded;
			args.ActionGroup.SignalRemoved += OnSignalRemoved;
			args.ActionGroup.SignalChanged += OnSignalChanged;
		}
		
		void OnGroupRemoved (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded -= OnSignalAdded;
			args.ActionGroup.SignalRemoved -= OnSignalRemoved;
			args.ActionGroup.SignalChanged -= OnSignalChanged;
		}
		
		void OnGroupChanged (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			NotifyChanged ();
		}
		
		void OnSignalAdded (object sender, SignalEventArgs args)
		{
			OnSignalAdded (args);
		}

		void OnSignalRemoved (object sender, SignalEventArgs args)
		{
			OnSignalRemoved (args);
		}

		void OnSignalChanged (object sender, SignalChangedEventArgs args)
		{
			OnSignalChanged (args);
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
			if (Project.Selection == Wrapped)
				Project.Selection = null;

			if (ParentWrapper != null)
				ParentWrapper.Delete (this);
			else
				Wrapped.Destroy ();
		}

		public override void Read (XmlElement elem, FileFormat format)
		{
			if (format == FileFormat.Native) {
				foreach (XmlElement groupElem in elem.SelectNodes ("action-group")) {
					ActionGroup actionGroup = new ActionGroup ();
					actionGroup.Read (Project, groupElem);
					if (actionGroups == null) {
						actionGroups = new ActionGroupCollection ();
						actionGroups.ActionGroupAdded += OnGroupAdded;
						actionGroups.ActionGroupRemoved += OnGroupRemoved;
						actionGroups.ActionGroupChanged += OnGroupChanged;
					} else
						actionGroups.Clear ();
					actionGroups.Add (actionGroup); 
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
				if (actionGroups != null) {
					foreach (ActionGroup actionGroup in actionGroups)
						elem.InsertBefore (actionGroup.Write (doc, format), elem.FirstChild);
				}
				return elem;
			}
			else {
				XmlElement elem = GladeUtils.ExportWidget (this, doc);
				GladeUtils.ExtractProperty (elem, "name", "");
				return elem;
			}
		}
		
		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			if (actionGroups != null) {
				// Create an UI manager
				uiManagerName = ctx.NewId ();
				CodeVariableDeclarationStatement uidec = new CodeVariableDeclarationStatement (
					typeof (Gtk.UIManager),
					uiManagerName,
					 new CodeObjectCreateExpression (typeof (Gtk.UIManager))
				);
				CodeVariableReferenceExpression uixp = new CodeVariableReferenceExpression (uiManagerName);
				ctx.Statements.Add (uidec);
				
				includedActionGroups = new ArrayList ();
				
				// Generate action group creation
				foreach (ActionGroup actionGroup in actionGroups) {
					
					// Create the action group
					string grpVar = ctx.NewId ();
					uidec = new CodeVariableDeclarationStatement (
						typeof (Gtk.ActionGroup),
						grpVar,
						actionGroup.GenerateObjectCreation (ctx)
					);
					ctx.Statements.Add (uidec);
					actionGroup.GenerateBuildCode (ctx, grpVar);
					
					// Insert the action group in the UIManager
					CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression (
						uixp,
						"InsertActionGroup",
						new CodeVariableReferenceExpression (grpVar),
						new CodePrimitiveExpression (includedActionGroups.Count)
					);
					ctx.Statements.Add (mi);
				
					includedActionGroups.Add (actionGroup);
				}
				
				// Adds the accel group to the window
				Window w = GetTopLevel () as Window;
				CodeMethodInvokeExpression ami = new CodeMethodInvokeExpression (
					new CodeVariableReferenceExpression (ctx.WidgetMap.GetWidgetId (w)),
					"AddAccelGroup",
					new CodePropertyReferenceExpression (
						uixp,
						"AccelGroup"
					)
				);
				ctx.Statements.Add (ami);
			}
			
			if (tooltip != null && tooltip.Length > 0)
				GetTopLevel().GenerateTooltip (ctx, this);
			
			base.GenerateBuildCode (ctx, varName);
		}
		
		internal protected override void GeneratePostBuildCode (GeneratorContext ctx, string varName)
		{
			base.GeneratePostBuildCode (ctx, varName);
			
			// The visible property is generated here to ensure that widgets are made visible
			// after they have been fully built
			
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
		
		protected override void GeneratePropertySet (GeneratorContext ctx, CodeVariableReferenceExpression var, PropertyDescriptor prop)
		{
			if (prop.Name != "Visible")
				base.GeneratePropertySet (ctx, var, prop);
		}
		
		protected CodeExpression GenerateUiManagerElement (GeneratorContext ctx, ActionTree tree)
		{
			Widget topLevel = GetTopLevel ();
			string uiName = topLevel.UIManagerName;
			if (uiName != null) {
				CodeVariableReferenceExpression uiManager = new CodeVariableReferenceExpression (uiName);
				if (topLevel.includedActionGroups == null)
					topLevel.includedActionGroups = new ArrayList ();
				
				// Add to the uimanager all action groups required by the 
				// actions of the tree
				
				foreach (ActionGroup grp in tree.GetRequiredGroups ()) {
					if (!topLevel.includedActionGroups.Contains (grp)) {
						// Insert the action group in the UIManager
						CodeMethodInvokeExpression mi = new CodeMethodInvokeExpression (
							uiManager,
							"InsertActionGroup",
							ctx.GenerateValue (grp, typeof(ActionGroup)),
							new CodePrimitiveExpression (topLevel.includedActionGroups.Count)
						);
						ctx.Statements.Add (mi);
						topLevel.includedActionGroups.Add (grp);
					}
				}
				
				tree.GenerateBuildCode (ctx, uiManager);
				return new CodeMethodInvokeExpression (
					uiManager,
					"GetWidget",
					new CodePrimitiveExpression ("/" + Wrapped.Name)
				);
			}
			return null;
		}
		
		void GenerateTooltip (GeneratorContext ctx, Widget widget)
		{
			if (generatedTooltips == null) {
				string tid = ctx.NewId ();
				CodeVariableDeclarationStatement vardec = new CodeVariableDeclarationStatement (
					typeof(Gtk.Tooltips),
					tid,
					new CodeObjectCreateExpression (typeof(Gtk.Tooltips))
				);
				ctx.Statements.Add (vardec);
				generatedTooltips = new CodeVariableReferenceExpression (tid);
			}
			ctx.Statements.Add (
				new CodeMethodInvokeExpression (
					generatedTooltips,
					"SetTip",
					new CodeVariableReferenceExpression (ctx.WidgetMap.GetWidgetId (widget)),
					new CodePrimitiveExpression (widget.Tooltip),
					new CodePrimitiveExpression (widget.Tooltip)
				)
			);
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
				return tooltip;
			}
			set {
				tooltip = value;
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
					
					// Keep the member name in sync with the widget name
					if (on == MemberName)
						MemberName = Wrapped.Name;
				}
			}
			else if (propertyName == "MemberName") {
				if (MemberName != oldMemberName) {
					string on = oldMemberName;
					oldMemberName = MemberName;
					OnMemberNameChanged (new WidgetNameChangedArgs (this, on, MemberName));
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
		
		protected virtual void OnMemberNameChanged (WidgetNameChangedArgs args)
		{
			NotifyChanged ();
			if (MemberNameChanged != null)
				MemberNameChanged (this, args);
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

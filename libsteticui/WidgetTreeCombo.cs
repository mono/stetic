
using System;
using Gtk;

namespace Stetic
{
	internal class WidgetTreeCombo: Button
	{
		Label label;
		Image image;
		Stetic.Wrapper.Widget rootWidget;
		Stetic.IProject project;
		Stetic.Wrapper.Widget selection;
		
		public WidgetTreeCombo ()
		{
			label = new Label ();
			image = new Gtk.Image ();
			
			HBox bb = new HBox ();
			bb.PackStart (image, false, false, 3);
			bb.PackStart (label, true, true, 3);
			label.Xalign = 0;
			label.HeightRequest = 24;
			bb.PackStart (new VSeparator (), false, false, 1);
			bb.PackStart (new Arrow (ArrowType.Down, ShadowType.None), false, false, 1);
			Child = bb;
			this.WidthRequest = 300;
			Sensitive = false;
		}
		
		public Stetic.Wrapper.Widget RootWidget {
			get { return rootWidget; }
			set {
				rootWidget = value;
				Sensitive = rootWidget != null;
				if (rootWidget != null)
					project = rootWidget.Project;
				Update ();
			}
		}
		
		public void SetSelection (Stetic.Wrapper.Widget widget) 
		{
			selection = widget;
			Update ();
		}
		
		void Update ()
		{
			if (selection != null) {
				label.Text = selection.Wrapped.Name;
				image.Pixbuf = selection.ClassDescriptor.Icon;
				image.Show ();
			} else {
				label.Text = "             ";
				image.Hide ();
			}
		}
		
		protected override void OnPressed ()
		{
			base.OnPressed ();

			Gtk.Menu menu = new Gtk.Menu ();
			FillCombo (menu, RootWidget.Wrapped, 0);
			menu.ShowAll ();
			menu.Popup (null, null, new Gtk.MenuPositionFunc (OnPosition), 0, Gtk.Global.CurrentEventTime);
		}
		
		void OnPosition (Gtk.Menu menu, out int x, out int y, out bool pushIn)
		{
			ParentWindow.GetOrigin (out x, out y);
			x += Allocation.X;
			y += Allocation.Y + Allocation.Height;
			pushIn = true;
		}
		
		void FillCombo (Menu menu, Gtk.Widget widget, int level)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			if (wrapper == null) return;
			
			if (!wrapper.Unselectable) {
				MenuItem item = new WidgetMenuItem (widget);
				item.Activated += new EventHandler (OnItemSelected);
				
				HBox box = new HBox ();
				Gtk.Image img = new Gtk.Image (wrapper.ClassDescriptor.Icon);
				img.Xalign = 1;
				img.WidthRequest = level*30;
				box.PackStart (img, false, false, 0);
				
				Label lab = new Label ();
				if (widget == project.Selection)
					lab.Markup = "<b>" + widget.Name + "</b>";
				else
					lab.Text = widget.Name;
					
				box.PackStart (lab, false, false, 3);
				item.Child = box;
				menu.Append (item);
			} else
				level--;
			
			Gtk.Container cc = widget as Gtk.Container;
			if (cc != null && wrapper.ClassDescriptor.AllowChildren) {
				foreach (Gtk.Widget child in cc.Children)
					FillCombo (menu, child, level + 1);
			}
		}
		
		void OnItemSelected (object sender, EventArgs args)
		{
			WidgetMenuItem item = (WidgetMenuItem) sender;
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (item.Widget);
			wrapper.Select ();
		}
	}
	
	class WidgetMenuItem: MenuItem
	{
		internal Gtk.Widget Widget;
		
		public WidgetMenuItem (Gtk.Widget widget)
		{
			this.Widget = widget;
		}
	}
}

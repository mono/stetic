
using System;

namespace Stetic
{
	public class WidgetPropertyTree: PropertyTree, IObjectViewer
	{
		Project project;
		Stetic.ObjectWrapper selection;
		Stetic.ObjectWrapper newSelection;
		Stetic.Wrapper.Container.ContainerChild packingSelection;
		
		public event EventHandler ObjectChanged;
		
		public WidgetPropertyTree (): this (null)
		{
			PreviewBox.DefaultPropertyViewer = this;
		}
		
		public WidgetPropertyTree (Project project)
		{
			PreviewBox.DefaultPropertyViewer = this;
			Stetic.Registry.RegistryChanging += new EventHandler (OnRegistryChanging);
			Project = project;
		}

		public Project Project {
			get { return project; }
			set {
				if (project != null)
					project.SelectionChanged -= Selected;
					
				project = value;
				if (project != null)
					project.SelectionChanged += Selected;
					
				Selected (null, null);
			}
		}
		
		public override void Clear ()
		{
			base.Clear ();
			Wrapper.Widget selWidget = selection as Wrapper.Widget;
			if (selWidget != null)
				selWidget.Notify -= Notified;
			if (packingSelection != null)
				packingSelection.Notify -= Notified;
		}
		
		protected override void OnObjectChanged ()
		{
			if (selection != null)
				selection.NotifyChanged ();
		}
		
		public object TargetObject {
			get { return selection.Wrapped; }
			set {
				newSelection = ObjectWrapper.Lookup (value);
				GLib.Timeout.Add (50, new GLib.TimeoutHandler (SelectedHandler));
			}
		}
		
		void Selected (object s, Wrapper.WidgetEventArgs args)
		{
			TargetObject = args != null && args.Widget != null? args.Widget.Wrapped : null;
		}
		
		bool SelectedHandler ()
		{
			SaveStatus ();
			
			Clear ();
			
			selection = newSelection;
			if (selection == null || selection.Wrapped is ErrorWidget) {
				return false;
			}

			Wrapper.Widget selWidget = selection as Wrapper.Widget;
			if (selWidget != null) {
				selWidget.Notify += Notified;
			
				PropertyDescriptor name = (PropertyDescriptor)Registry.LookupClassByName ("Gtk.Widget") ["Name"];
				AppendProperty (name, selection.Wrapped);
			}

			AddProperties (selection.ClassDescriptor.ItemGroups, selection.Wrapped);
			
			if (selWidget != null) {
				packingSelection = Stetic.Wrapper.Container.ChildWrapper (selWidget);
				if (packingSelection != null) {
					ClassDescriptor childklass = packingSelection.ClassDescriptor;
					if (childklass.ItemGroups.Count > 0) {
						AddProperties (childklass.ItemGroups, packingSelection.Wrapped);
						packingSelection.Notify += Notified;
					}
				}
			}
			
			RestoreStatus ();
			return false;
		}
		
		void Notified (object wrapper, string propertyName)
		{
			Update ();
		}
		
		void OnRegistryChanging (object o, EventArgs args)
		{
			Clear ();
		}
	}
	
}

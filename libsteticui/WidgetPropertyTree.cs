
using System;

namespace Stetic
{
	public class WidgetPropertyTree: PropertyTree
	{
		Project project;
		Stetic.Wrapper.Widget selection;
		Stetic.Wrapper.Widget newSelection;
		Stetic.Wrapper.Container.ContainerChild packingSelection;
		
		public WidgetPropertyTree (): this (null)
		{
		}
		
		public WidgetPropertyTree (Project project)
		{
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
			if (selection != null)
				selection.Notify -= Notified;
			if (packingSelection != null)
				packingSelection.Notify -= Notified;
		}
		
		void Selected (object s, Wrapper.WidgetEventArgs args)
		{
			newSelection = args != null ? args.Widget : null;
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (SelectedHandler));
		}
		
		bool SelectedHandler ()
		{
			SaveStatus ();
			
			Clear ();
			
			selection = newSelection;
			if (selection == null || selection.Wrapped is ErrorWidget) {
				return false;
			}

			selection.Notify += Notified;
			
			PropertyDescriptor name = (PropertyDescriptor)Registry.LookupClassByName ("Gtk.Widget") ["Name"];
			AppendProperty (name, selection.Wrapped);

			AddProperties (selection.ClassDescriptor.ItemGroups, selection.Wrapped);
			
			packingSelection = Stetic.Wrapper.Container.ChildWrapper (selection);
			if (packingSelection != null) {
				ClassDescriptor childklass = packingSelection.ClassDescriptor;
				if (childklass.ItemGroups.Count > 0) {
					AddProperties (childklass.ItemGroups, packingSelection.Wrapped);
					packingSelection.Notify += Notified;
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

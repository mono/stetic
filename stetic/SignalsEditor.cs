using System;
using System.Collections;
using Stetic.Wrapper;

namespace Stetic
{
	public class SignalsEditor: Gtk.ScrolledWindow
	{
		Gtk.TreeView tree;
		Gtk.TreeStore store;
		
		Project project;
		Stetic.Wrapper.Widget selection;
		bool internalChange;
		
		const int ColSignal = 0;
		const int ColHandler = 1;
		const int ColAfter = 2;
		const int ColHasHandler = 3;
		const int ColIsSignal = 4;
		const int ColDescriptorObject = 5;
		const int ColSignalObject = 6;
		const int ColSignalTextWeight = 7;
		
		public SignalsEditor ()
		{
			tree = new Gtk.TreeView ();
			store = new Gtk.TreeStore (typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(SignalDescriptor), typeof(Signal), typeof(int));
			tree.Model = store;
			tree.RowActivated += new Gtk.RowActivatedHandler (OnRowActivated);
			
			Gtk.CellRendererText crtSignal = new Gtk.CellRendererText ();
			
			Gtk.CellRendererText crtHandler = new Gtk.CellRendererText ();
			crtHandler.Editable = true;
			crtHandler.Edited += new Gtk.EditedHandler (OnHandlerEdited);
			
			Gtk.CellRendererToggle crtogAfter = new Gtk.CellRendererToggle ();
			crtogAfter.Activatable = true;
			crtogAfter.Toggled += new Gtk.ToggledHandler (OnAfterToggled);
			
			tree.AppendColumn ("Signal", crtSignal, "text", ColSignal, "weight", ColSignalTextWeight);
			tree.AppendColumn ("Handler", crtHandler, "markup", ColHandler, "visible", ColIsSignal);
			tree.AppendColumn ("After", crtogAfter, "active", ColAfter, "visible", ColHasHandler);
			
			Add (tree);
			ShowAll ();
		}
		
		public SignalsEditor (Project project): this ()
		{
			Project = project;
		}
		
		public Project Project {
			get { return project; }
			set {
				if (project != null) {
					project.Selected -= OnWidgetSelected;
					project.SignalAdded -= new SignalEventHandler (OnSignalAddedOrRemoved);
					project.SignalRemoved -= new SignalEventHandler (OnSignalAddedOrRemoved);
					project.SignalChanged -= new SignalChangedEventHandler (OnSignalChanged);
				}
					
				project = value;
				
				project.Selected += OnWidgetSelected;
				project.SignalAdded += new SignalEventHandler (OnSignalAddedOrRemoved);
				project.SignalRemoved += new SignalEventHandler (OnSignalAddedOrRemoved);
				project.SignalChanged += new SignalChangedEventHandler (OnSignalChanged);
				
				OnWidgetSelected (null, null);
			}
		}
		
		void OnWidgetSelected (Gtk.Widget widget, ProjectNode node)
		{
			selection = Stetic.Wrapper.Widget.Lookup (widget);
			RefreshTree ();
		}
		
		void OnSignalAddedOrRemoved (object sender, SignalEventArgs args)
		{
			if (!internalChange && args.Widget == selection)
				RefreshTree ();
		}

		void OnSignalChanged (object sender, SignalChangedEventArgs args)
		{
			if (!internalChange && args.Widget == selection)
				RefreshTree ();
		}
		
		void RefreshTree ()
		{
			ArrayList status = SaveStatus ();
			store.Clear ();
			
			if (selection == null)
				return;

			ClassDescriptor klass = Registry.LookupClass (selection.Wrapped.GetType ());
			
			foreach (ItemGroup group in klass.SignalGroups) {
				Gtk.TreeIter iter = store.AppendNode ();
				store.SetValue (iter, ColSignal, group.Label);
				if (FillGroup (iter, group))
					store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Bold);
				else
					store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Normal);
			}
			RestoreStatus (status);
		}
		
		bool FillGroup (Gtk.TreeIter parentIter, ItemGroup group)
		{
			bool hasSignals = false;
			foreach (SignalDescriptor sd in group) {
				bool foundSignal = false;
				Gtk.TreeIter signalParent = parentIter;
				
				foreach (Signal signal in selection.Signals) {
					if (signal.SignalDescriptor != sd) continue;

					Gtk.TreeIter iter = store.AppendNode (signalParent);
					if (!foundSignal) {
						signalParent = iter;
						store.SetValue (iter, ColSignal, sd.Name);
						store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Bold);
						foundSignal = true;
					}
					SetSignalData (iter, signal);
				}
				
				Gtk.TreeIter signalIter = store.AppendNode (signalParent);
				SetEmptySingalRow (signalIter, sd, !foundSignal);
				
				hasSignals = hasSignals || foundSignal;
			}
			return hasSignals;
		}
		
		void SetSignalData (Gtk.TreeIter iter, Signal signal)
		{
			store.SetValue (iter, ColHandler, signal.Handler);
			store.SetValue (iter, ColAfter, false);
			store.SetValue (iter, ColHasHandler, true);
			store.SetValue (iter, ColIsSignal, true);
			store.SetValue (iter, ColDescriptorObject, signal.SignalDescriptor);
			store.SetValue (iter, ColSignalObject, signal);
		}
		
		void SetEmptySingalRow (Gtk.TreeIter signalIter, SignalDescriptor sd, bool showName)
		{
			if (showName)
				store.SetValue (signalIter, ColSignal, sd.Name);
			store.SetValue (signalIter, ColHandler, "<i><span foreground=\"grey\">" + EmptyHandlerText + "</span></i>");
			store.SetValue (signalIter, ColHasHandler, false);
			store.SetValue (signalIter, ColIsSignal, true);
			store.SetValue (signalIter, ColDescriptorObject, sd);
			store.SetValue (signalIter, ColSignalTextWeight, (int) Pango.Weight.Normal);
		}
		
		void OnRowActivated (object sender, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			if (!store.GetIter (out iter, args.Path))
				return;
			
			SignalDescriptor sd = GetSignalDescriptor (iter);
			if (sd != null)
				AddHandler (iter, "On" + sd.Name);
		}
		
		void OnHandlerEdited (object sender, Gtk.EditedArgs args)
		{
			if (args.NewText == EmptyHandlerText)
				return;

			Gtk.TreeIter iter;
			if (!store.GetIterFromString (out iter, args.Path))
				return;
				
			AddHandler (iter, args.NewText);
		}
		
		void AddHandler (Gtk.TreeIter iter, string name)
		{
			internalChange = true;
			
			Gtk.TreeIter piter = iter;
			while (store.IterDepth (piter) != 0)
				store.IterParent (out piter, piter);
			
			Signal signal = GetSignal (iter);
			if (signal == null) {
				if (name != "") {
					SignalDescriptor sd = (SignalDescriptor) store.GetValue (iter, ColDescriptorObject);
					signal = new Signal (sd);
					signal.Handler = name;
					selection.Signals.Add (signal);
					SetSignalData (iter, signal);
					store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Bold);
					if (store.IterDepth (iter) == 1)
						SetEmptySingalRow (store.AppendNode (iter), signal.SignalDescriptor, false);
					else {
						store.IterParent (out iter, iter);
						SetEmptySingalRow (store.AppendNode (iter), signal.SignalDescriptor, false);
					}
				}
			} else {
				if (name != "") {
					signal.Handler = name;
					store.SetValue (iter, ColHandler, signal.Handler);
				} else {
					selection.Signals.Remove (signal);
					if (store.IterDepth (iter) == 1) {
						if (store.IterNChildren (iter) == 1) {
							SetEmptySingalRow (iter, signal.SignalDescriptor, true);
							// Remove the empty row
							store.IterChildren (out iter, iter);
							store.Remove (ref iter);
						} else {
							Gtk.TreeIter citer;
							store.IterChildren (out citer, iter);
							Signal csignal = GetSignal (citer);
							store.Remove (ref citer);
							SetSignalData (iter, csignal);
							if (store.IterNChildren (iter) == 1)
								tree.CollapseRow (store.GetPath (iter));
						}
					} else
						store.Remove (ref iter);
				}
			}
			UpdateGroupStatus (piter);
			internalChange = false;
		}
		
		void OnAfterToggled (object o, Gtk.ToggledArgs args)
		{
			Gtk.TreeIter it;
			if (store.GetIterFromString (out it, args.Path)) {
				Signal signal = GetSignal (it);
				if (signal != null) {
					internalChange = true;
					signal.After = !signal.After;
					internalChange = false;
					store.SetValue (it, ColAfter, signal.After);
				}
			}
		}
		
		void UpdateGroupStatus (Gtk.TreeIter iter)
		{
			Gtk.TreeIter signalIter;
			if (store.IterChildren (out signalIter, iter)) {
				do {
					if (store.IterNChildren (signalIter) > 0) {
						store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Bold);
						return;
					}
				} while (store.IterNext (ref signalIter));
			}
			store.SetValue (iter, ColSignalTextWeight, (int) Pango.Weight.Normal);
		}
		
		Signal GetSignal (Gtk.TreeIter iter)
		{
			if (! (bool) store.GetValue (iter, ColHasHandler))
				return null;
			return (Signal) store.GetValue (iter, ColSignalObject);
		}
		
		SignalDescriptor GetSignalDescriptor (Gtk.TreeIter iter)
		{
			if (! (bool) store.GetValue (iter, ColIsSignal))
				return null;
			return (SignalDescriptor) store.GetValue (iter, ColDescriptorObject);
		}
		
		ArrayList SaveStatus ()
		{
			ArrayList list = new ArrayList ();
			
			Gtk.TreeIter it; 
			if (!store.GetIterFirst (out it))
				return list;
			
			do {
				SaveStatus (list, "", it);
			} while (store.IterNext (ref it));
			
			return list;
		}
		
		void SaveStatus (ArrayList list, string path, Gtk.TreeIter iter)
		{
			string basePath = path + "/" + store.GetValue (iter, ColSignal);
				
			if (tree.GetRowExpanded (store.GetPath (iter)))
				list.Add (basePath);
			
			if (store.IterChildren (out iter, iter)) {
				do {
					SaveStatus (list, basePath, iter);
				} while (store.IterNext (ref iter));
			}
		}
		
		void RestoreStatus (ArrayList list)
		{
			foreach (string namePath in list) {
				string[] names = namePath.Split ('/');
				
				Gtk.TreeIter iter = Gtk.TreeIter.Zero;

				bool found = true;
				foreach (string name in names) {
					if (name == "") continue;
					if (!FindChildByName (name, ref iter)) {
						found = false;
						break;
					}
				}
				
				if (found)
					tree.ExpandRow (store.GetPath (iter), false);
			}
		}
		
		bool FindChildByName (string name, ref Gtk.TreeIter iter)
		{
			if (iter.Equals (Gtk.TreeIter.Zero)) {
				if (!store.GetIterFirst (out iter))
					return false;
			} else if (!store.IterChildren (out iter, iter))
				return false;
			
			do {
				if (name == (string) store.GetValue (iter, ColSignal))
					return true;
			}
			while (store.IterNext (ref iter));
			
			return false;
		}
		
		string EmptyHandlerText {
			get { return "Click here to add a new handler"; } 
		}
	}
	
}

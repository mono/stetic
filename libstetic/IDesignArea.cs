
using System;

namespace Stetic
{
	public interface IDesignArea
	{
		IObjectSelection SetSelection (Gtk.Widget widget, object selectedInstance);
		void ResetSelection (Gtk.Widget widget);
		bool IsSelected (Gtk.Widget widget);
		IObjectSelection GetSelection ();
		IObjectSelection GetSelection (Gtk.Widget widget);

		void AddWidget (Gtk.Widget w, int x, int y);
		void RemoveWidget (Gtk.Widget w);
		void MoveWidget (Gtk.Widget w, int x, int y);
		Gdk.Rectangle GetCoordinates (Gtk.Widget w);
		
		event EventHandler SelectionChanged;
	}
	
	public interface IObjectViewer
	{
		object TargetObject { get; set; }
	}
	
	public interface IObjectSelection: IDisposable
	{
		Gtk.Widget Widget { get; }
		object DataObject { get; }
		
		event DragDelegate Drag;
		event EventHandler Disposed;
	}
}

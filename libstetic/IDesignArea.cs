
using System;

namespace Stetic
{
	public interface IDesignArea
	{
		IObjectSelection SetSelection (Gtk.Widget widget, object selectedInstance);
		void ResetSelection (Gtk.Widget widget);
		bool IsSelected (Gtk.Widget widget);

		void AddWidget (Gtk.Widget w, int x, int y);
		void RemoveWidget (Gtk.Widget w);
		void MoveWidget (Gtk.Widget w, int x, int y);
		Gdk.Rectangle GetCoordinates (Gtk.Widget w);
	}
	
	public interface IObjectViewer
	{
		object TargetObject { get; set; }
	}
	
	public interface IObjectSelection: IDisposable
	{
		event DragDelegate Drag;
		event EventHandler Disposed;
	}
}

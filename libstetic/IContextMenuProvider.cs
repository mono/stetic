using System;
using System.Collections;

namespace Stetic {

	public delegate void ContextMenuItemDelegate (IWidgetSite context);

	public struct ContextMenuItem {
		public string Label;
		public ContextMenuItemDelegate Callback;
		public bool Enabled;

		public ContextMenuItem (string label, ContextMenuItemDelegate callback) : this (label, callback, true) {}

		public ContextMenuItem (string label, ContextMenuItemDelegate callback, bool enabled)
		{
			Label = label;
			Callback = callback;
			Enabled = enabled;
		}
	}

	public interface IContextMenuProvider {
		IEnumerable ContextMenuItems (IWidgetSite context);
	}
}

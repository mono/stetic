using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Button", "button.png", ObjectWrapperType.Widget)]
	public class Button : Stetic.Wrapper.Widget, Stetic.IContextMenuProvider {

		public static PropertyGroup ButtonProperties;
		public static PropertyGroup ButtonExtraProperties;

		static Button () {
			ButtonProperties = new PropertyGroup ("Button Properties",
							      typeof (Stetic.Wrapper.Button),
							      typeof (Gtk.Button),
							      "UseStock",
							      "StockId",
							      "Label");
			ButtonProperties["StockId"].DependsOn (ButtonProperties["UseStock"]);
			ButtonProperties["Label"].DependsInverselyOn (ButtonProperties["UseStock"]);

			ButtonExtraProperties = new PropertyGroup ("Extra Button Properties",
								   typeof (Gtk.Button),
								   "FocusOnClick",
								   "UseUnderline",
								   "Relief",
								   "Xalign",
								   "Yalign");

			groups = new PropertyGroup[] {
				ButtonProperties, ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Button (IStetic stetic) : this (stetic, new Gtk.Button (Gtk.Stock.Ok)) {}

		public Button (IStetic stetic, Gtk.Button button) : base (stetic, button)
		{
			if (button.UseStock) {
				stockId = button.Label;
				label = "";
			} else {
				label = button.Label;
				stockId = Gtk.Stock.Ok;
			}
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		private Gtk.Button button {
			get {
				return (Gtk.Button)Wrapped;
			}
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;
			WidgetSite site = button.Child as WidgetSite;

			bool hasLabel = (site == null);
			bool isEmpty = (site != null) && !site.Occupied;

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Remove Button Contents", new ContextMenuItemDelegate (RemoveContents), !isEmpty),
				new ContextMenuItem ("Restore Button Label", new ContextMenuItemDelegate (RestoreLabel), !hasLabel)
			};
			return items;
		}

		void RemoveContents (IWidgetSite context)
		{
			if (button.Child != null)
				button.Remove (button.Child);

			WidgetSite site = stetic.CreateWidgetSite ();
			site.Show ();
			button.Add (site);
		}

		void RestoreLabel (IWidgetSite context)
		{
			if (button.Child != null)
				button.Remove (button.Child);

			if (UseStock)
				button.Label = stockId;
			else
				button.Label = label;
		}

		string stockId;
		string label;

		public bool UseStock {
			get {
				return button.UseStock;
			}
			set {
				if (value)
					button.Label = stockId;
				else
					button.Label = label;
				button.UseStock = value;
			}
		}

		[Editor (typeof (Stetic.Editor.StockItem), typeof (Gtk.Widget))]
		[DefaultValue ("gtk-ok")]
		public string StockId {
			get {
				return stockId;
			}
			set {
				stockId = value;
				if (UseStock)
					button.Label = value;

				Gtk.StockItem item = Gtk.Stock.Lookup (value);
				if (item.Label != null)
					label = item.Label;
			}
		}

		public string Label {
			get {
				return label;
			}
			set {
				label = value;
				if (!UseStock)
					button.Label = value;
			}
		}
	}
}

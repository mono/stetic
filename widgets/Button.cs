using Gtk;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Widget {

	[WidgetWrapper ("Button", "button.png")]
	public class Button : Gtk.Button, Stetic.IObjectWrapper, Stetic.IContextMenuProvider {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ButtonProperties;
		public static PropertyGroup ButtonExtraProperties;

		static Button () {
			ButtonProperties = new PropertyGroup ("Button Properties",
							      typeof (Stetic.Widget.Button),
							      "UseStock",
							      "StockId",
							      "Label");
			ButtonProperties["StockId"].DependsOn (ButtonProperties["UseStock"]);
			ButtonProperties["Label"].DependsInverselyOn (ButtonProperties["UseStock"]);

			ButtonExtraProperties = new PropertyGroup ("Extra Button Properties",
								   typeof (Stetic.Widget.Button),
								   "FocusOnClick",
								   "UseUnderline",
								   "Relief",
								   "Xalign",
								   "Yalign");

			groups = new PropertyGroup[] {
				ButtonProperties, ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		IStetic stetic;

		public Button (IStetic stetic)
		{
			this.stetic = stetic;
			UseStock = UseUnderline = true;
			StockId = Gtk.Stock.Ok;
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;
			WidgetSite site = Child as WidgetSite;

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
			if (Child != null)
				Remove (Child);

			WidgetSite site = stetic.CreateWidgetSite ();
			site.Show ();
			Add (site);
		}

		void RestoreLabel (IWidgetSite context)
		{
			if (Child != null)
				Remove (Child);

			if (UseStock)
				base.Label = stockId;
			else
				base.Label = label;
		}

		string stockId;
		string label;

		public new bool UseStock {
			get {
				return base.UseStock;
			}
			set {
				if (value)
					base.Label = stockId;
				else
					base.Label = label;
				base.UseStock = value;
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
					base.Label = value;

				StockItem item = Gtk.Stock.Lookup (value);
				if (item.Label != null)
					Label = item.Label;
			}
		}

		public new string Label {
			get {
				return label;
			}
			set {
				label = value;
				if (!UseStock)
					base.Label = value;
			}
		}
	}
}

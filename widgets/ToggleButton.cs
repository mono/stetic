using Gtk;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Widget {

	[WidgetWrapper ("Toggle Button", "togglebutton.png")]
	public class ToggleButton : Gtk.ToggleButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ToggleButtonProperties;

		static ToggleButton () {
			ToggleButtonProperties = new PropertyGroup ("Toggle Button Properties",
								    typeof (Stetic.Widget.ToggleButton),
								    "UseStock",
								    "StockId",
								    "Label",
								    "Active",
								    "Inconsistent");
			ToggleButtonProperties["StockId"].DependsOn (ToggleButtonProperties["UseStock"]);
			ToggleButtonProperties["Label"].DependsInverselyOn (ToggleButtonProperties["UseStock"]);

			groups = new PropertyGroup[] {
				ToggleButtonProperties,
				Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ToggleButton (IStetic stetic) : base ("Toggle")
		{
			UseStock = false;
			UseUnderline = true;
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

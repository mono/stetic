using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Toggle Button", "togglebutton.png")]
	public class ToggleButton : Gtk.ToggleButton, Stetic.IObjectWrapper, Stetic.IPropertySensitizer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ToggleButtonProperties;

		static ToggleButton () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "UseStock"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.ToggleButton), "StockId"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.ToggleButton), typeof (Gtk.Button), "Label"),
//				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "Icon")
				new PropertyDescriptor (typeof (Gtk.ToggleButton), "Active"),
				new PropertyDescriptor (typeof (Gtk.ToggleButton), "Inconsistent"),
			};				
			ToggleButtonProperties = new PropertyGroup ("Toggle Button Properties", props);

			groups = new PropertyGroup[] {
				ToggleButtonProperties,
				Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ToggleButton () : base ("Toggle")
		{
			UseStock = false;
			UseUnderline = true;
			Notify.Add (this, new NotifyDelegate (Notified));
		}

		void Notified (ParamSpec pspec)
		{
			if (pspec.Name == "use-stock") {
				EmitSensitivityChanged ("StockId", UseStock);
				EmitSensitivityChanged ("Label", !UseStock);
				if (UseStock)
					base.Label = stockId;
				else
					base.Label = label;
			}
		}

		string stockId;
		string label;

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

				if (LabelChanged != null)
					LabelChanged (this, EventArgs.Empty);
			}
		}

		public event EventHandler LabelChanged;

		public IEnumerable InsensitiveProperties {
			get {
				if (UseStock)
					return new string[] { "Label" };
				else
					return new string[0];
			}
		}

		public event SensitivityChangedDelegate SensitivityChanged;

		void EmitSensitivityChanged (string property, bool sensitivity)
		{
			if (SensitivityChanged != null)
				SensitivityChanged (property, sensitivity);
		}
	}
}

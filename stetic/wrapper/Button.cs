using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	public class Button : Gtk.Button, Stetic.IObjectWrapper, Stetic.IPropertySensitizer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ButtonProperties;
		public static PropertyGroup ButtonExtraProperties;

		static Button () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "UseStock"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "StockId"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "Label"),
//				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "Icon")
			};				
			ButtonProperties = new PropertyGroup ("Button Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "FocusOnClick"),
				new PropertyDescriptor (typeof (Gtk.Button), "UseUnderline"),
				new PropertyDescriptor (typeof (Gtk.Button), "Relief"),
				new PropertyDescriptor (typeof (Gtk.Button), "Xalign"),
				new PropertyDescriptor (typeof (Gtk.Button), "Yalign"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth")
			};
			ButtonExtraProperties = new PropertyGroup ("Extra Button Properties", props);

			groups = new PropertyGroup[] {
				ButtonProperties, ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Button ()
		{
			UseStock = UseUnderline = true;
			StockId = Gtk.Stock.Ok;
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

		public IEnumerable InsensitiveProperties ()
		{
			if (UseStock)
				return new string[] { "Label" };
			else
				return new string[0];
		}

		public event SensitivityChangedDelegate SensitivityChanged;

		void EmitSensitivityChanged (string property, bool sensitivity)
		{
			if (SensitivityChanged != null)
				SensitivityChanged (property, sensitivity);
		}
	}
}

using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Button", "button.png")]
	public class Button : Gtk.Button, Stetic.IObjectWrapper, Stetic.IPropertySensitizer, Stetic.IContextMenuProvider {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ButtonProperties;
		public static PropertyGroup ButtonExtraProperties;

		static Button () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "UseStock"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "StockId"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), typeof (Gtk.Button), "Label"),
			};				
			ButtonProperties = new PropertyGroup ("Button Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "FocusOnClick"),
				new PropertyDescriptor (typeof (Gtk.Button), "UseUnderline"),
				new PropertyDescriptor (typeof (Gtk.Button), "Relief"),
				new PropertyDescriptor (typeof (Gtk.Button), "Xalign"),
				new PropertyDescriptor (typeof (Gtk.Button), "Yalign"),
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

			WidgetSite site = new WidgetSite ();
			site.Show ();
			Add (site);

			EmitSensitivityChanged ("UseStock", false);
			EmitSensitivityChanged ("StockId", false);
			EmitSensitivityChanged ("Label", false);
		}

		void RestoreLabel (IWidgetSite context)
		{
			if (Child != null)
				Remove (Child);

			if (UseStock)
				base.Label = stockId;
			else
				base.Label = label;

			EmitSensitivityChanged ("UseStock", true);
			EmitSensitivityChanged ("StockId", true);
			EmitSensitivityChanged ("Label", true);
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
				if (Child is WidgetBox)
					return new string[] { "UseStock", "StockId", "Label" };
				else if (UseStock)
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

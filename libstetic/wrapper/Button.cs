using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Button", "button.png", ObjectWrapperType.Widget)]
	public class Button : Stetic.Wrapper.Widget {

		public static ItemGroup ButtonProperties;
		public static ItemGroup ButtonExtraProperties;

		static Button () {
			ButtonProperties = new ItemGroup ("Button Properties",
							  typeof (Stetic.Wrapper.Button),
							  typeof (Gtk.Button),
							  "UseStock",
							  "StockId",
							  "Label",
							  "RemoveContents",
							  "RestoreLabel");
			ButtonProperties["StockId"].DependsOn (ButtonProperties["UseStock"]);
			ButtonProperties["Label"].DependsInverselyOn (ButtonProperties["UseStock"]);

			PropertyDescriptor hasLabel = new PropertyDescriptor (typeof (Stetic.Wrapper.Button),
									      typeof (Gtk.Button),
									      "HasLabel");
			ButtonProperties["UseStock"].DependsOn (hasLabel);
			ButtonProperties["StockId"].DependsOn (hasLabel);
			ButtonProperties["Label"].DependsOn (hasLabel);
			ButtonProperties["RestoreLabel"].DependsInverselyOn (hasLabel);

			PropertyDescriptor hasContents = new PropertyDescriptor (typeof (Stetic.Wrapper.Button),
										 typeof (Gtk.Button),
										 "HasContents");
			ButtonProperties["RemoveContents"].DependsOn (hasContents);

			ButtonExtraProperties = new ItemGroup ("Extra Button Properties",
							       typeof (Gtk.Button),
							       "FocusOnClick",
							       "UseUnderline",
							       "Relief",
							       "Xalign",
							       "Yalign");

			RegisterItems (typeof (Stetic.Wrapper.Button),
				       ButtonProperties,
				       ButtonExtraProperties,
				       Widget.CommonWidgetProperties);

			ItemGroup contextMenu = new ItemGroup (null,
							       typeof (Stetic.Wrapper.Button),
							       typeof (Gtk.Button),
							       "RemoveContents",
							       "RestoreLabel");
			contextMenu["RemoveContents"].DependsOn (hasContents);
			contextMenu["RestoreLabel"].DependsInverselyOn (hasLabel);
			RegisterContextMenu (typeof (Stetic.Wrapper.Button), contextMenu);
		}

		public Button (IStetic stetic) : this (stetic, new Gtk.Button (Gtk.Stock.Ok)) {}

		public Button (IStetic stetic, Gtk.Button button) : base (stetic, button)
		{
			if (button.UseStock) {
				stockId = button.Label;
				label = button.Name;
			} else {
				label = button.Name;
				stockId = Gtk.Stock.Ok;
			}
		}

		private Gtk.Button button {
			get {
				return (Gtk.Button)Wrapped;
			}
		}

		// true if the button has a label rather than custom contents
		public bool HasLabel {
			get {
				return (button.Child as WidgetSite) == null;
			}
		}

		// true if the button has *anything* in it
		public bool HasContents {
			get {
				WidgetSite site = button.Child as WidgetSite;
				return (site == null) || site.Occupied;
			}
		}

		[Command ("Remove Button Contents")]
		void RemoveContents ()
		{
			if (button.Child != null)
				button.Remove (button.Child);

			WidgetSite site = stetic.CreateWidgetSite ();
			site.Show ();
			site.OccupancyChanged += delegate (WidgetSite site) {
				EmitNotify ("HasContents");
			};
			button.Add (site);

			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
		}

		[Command ("Restore Button Label")]
		void RestoreLabel ()
		{
			if (button.Child != null)
				button.Remove (button.Child);

			if (UseStock)
				button.Label = stockId;
			else
				button.Label = label;

			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
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

		[Editor (typeof (Stetic.Editor.StockItem))]
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

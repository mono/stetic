using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Button", "button.png", ObjectWrapperType.Widget)]
	public class Button : Container {

		public static new Type WrappedType = typeof (Gtk.Button);

		static new void Register (Type type)
		{
			if (type == typeof (Stetic.Wrapper.Button)) {
				ItemGroup props = AddItemGroup (type, "Button Properties",
								"UseStock",
								"StockId",
								"Label",
								"RemoveContents",
								"RestoreLabel");

				PropertyDescriptor hasLabel =
					new PropertyDescriptor (typeof (Stetic.Wrapper.Button),
								typeof (Gtk.Button),
								"HasLabel");
				PropertyDescriptor hasContents =
					new PropertyDescriptor (typeof (Stetic.Wrapper.Button),
								typeof (Gtk.Button),
								"HasContents");

				props["UseStock"].DependsOn (hasLabel);
				props["StockId"].DependsOn (hasLabel);
				props["StockId"].DependsOn (props["UseStock"]);
				props["Label"].DependsOn (hasLabel);
				props["Label"].DependsInverselyOn (props["UseStock"]);
				props["RestoreLabel"].DependsInverselyOn (hasLabel);
				props["RemoveContents"].DependsOn (hasContents);

				props = AddContextMenuItems (type,
							     "RemoveContents",
							     "RestoreLabel");
				props["RemoveContents"].DependsOn (hasContents);
				props["RestoreLabel"].DependsInverselyOn (hasLabel);
			}

			AddItemGroup (type, "Extra Button Properties",
				      "FocusOnClick",
				      "UseUnderline",
				      "Relief",
				      "Xalign",
				      "Yalign",
				      "BorderWidth");
		}

		public static new Gtk.Button CreateInstance ()
		{
			return new Gtk.Button (Gtk.Stock.Ok);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				if (button.UseStock) {
					stockId = button.Label;
					label = button.Name;
				} else {
					label = button.Name;
					stockId = Gtk.Stock.Ok;
				}
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

		[Command ("Remove Button Contents", "Remove the current contents of the button")]
		void RemoveContents ()
		{
			if (button.Child != null)
				button.Remove (button.Child);

			WidgetSite site = CreateWidgetSite ();
			site.OccupancyChanged += delegate (WidgetSite site) {
				EmitNotify ("HasContents");
			};
			button.Add (site);

			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
		}

		[Command ("Restore Button Label", "Restore the button's label")]
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
		[Description ("Stock Item", "The stock icon and label to display in the button")]
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

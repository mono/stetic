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
								"Icon",
								"Label",
								"ResponseId",
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

				props["Icon"].DependsOn (hasLabel);
				props["Label"].DependsOn (hasLabel);
				props["RestoreLabel"].DependsInverselyOn (hasLabel);
				props["RemoveContents"].DependsOn (hasContents);

				PropertyDescriptor hasResponseId =
					new PropertyDescriptor (typeof (Stetic.Wrapper.Button),
								typeof (Gtk.Button),
								"HasResponseId");
				props["ResponseId"].VisibleIf (hasResponseId);

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
			if (!initialized)
				button.Label = button.Name;

			if (button.UseStock)
				Icon = "stock:" + button.Label;
			else {
				Icon = null;
				Label = button.Label;
			}
		}

		public override Widget GladeImportChild (string className, string id,
							 Hashtable props, Hashtable childprops)
		{
			stetic.GladeImportComplete += FixupGladeChildren;

			if (button.Child != null)
				button.Remove (button.Child);
			return base.GladeImportChild (className, id, props, childprops);
		}

		void FixupGladeChildren ()
		{
			stetic.GladeImportComplete -= FixupGladeChildren;

			WidgetSite site = button.Child as WidgetSite;
			Gtk.Alignment alignment = (site == null) ? null : site.Contents as Gtk.Alignment;
			if (alignment == null)
				return;

			site = alignment.Child as WidgetSite;
			Gtk.HBox box = (site == null) ? null : site.Contents as Gtk.HBox;
			if (box == null)
				return;

			Gtk.Widget[] children = box.Children;
			if (children == null || children.Length != 2)
				return;

			site = children[0] as WidgetSite;
			Gtk.Image image = (site == null) ? null : site.Contents as Gtk.Image;
			site = children[1] as WidgetSite;
			Gtk.Label label = (site == null) ? null : site.Contents as Gtk.Label;
			if (image == null || label == null)
				return;
			Stetic.Wrapper.Image iwrap = Stetic.ObjectWrapper.Lookup (image) as Stetic.Wrapper.Image;
			if (iwrap == null)
				return;

			if (iwrap.UseStock)
				Icon = "stock:" + iwrap.Stock;
			else
				Icon = "file:" + iwrap.File;
			Label = label.LabelProp;
		}

		private Gtk.Button button {
			get {
				return (Gtk.Button)Wrapped;
			}
		}

		// true if the button has an icon+label rather than custom contents
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
			ConstructChild ();
			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
		}

		Gtk.Image iconWidget;
		Gtk.Label labelWidget;
		string icon;
		string label;

		void ConstructChild ()
		{
			if (button.Child != null)
				button.Remove (button.Child);
			iconWidget = null;
			labelWidget = null;

			if (button.UseUnderline) {
				labelWidget = new Gtk.Label (label);
				labelWidget.MnemonicWidget = button;
			} else
				labelWidget = Gtk.Label.New (label);
				labelWidget.Show ();

			if (icon == null) {
				labelWidget.Xalign = button.Xalign;
				labelWidget.Yalign = button.Yalign;
				button.Add (labelWidget);
				return;
			}

			if (icon.StartsWith ("stock:"))
				iconWidget = new Gtk.Image (icon.Substring (6), Gtk.IconSize.Button);
			else if (icon.StartsWith ("file:"))
				iconWidget = new Gtk.Image (icon.Substring (5));
			else
				iconWidget = new Gtk.Image (Gtk.Stock.MissingImage, Gtk.IconSize.Button);

			Gtk.HBox box = new Gtk.HBox (false, 2);
			box.PackStart (iconWidget, false, false, 0);
			box.PackEnd (labelWidget, false, false, 0);

			Gtk.Alignment alignment = new Gtk.Alignment (button.Xalign, button.Yalign, 0.0f, 0.0f);
			alignment.Add (box);
			alignment.ShowAll ();

			button.Add (alignment);
		}

		[Editor (typeof (Stetic.Editor.Image))]
		[Description ("Icon", "The icon to display in the button")]
		public string Icon {
			get {
				return icon;
			}
			set {
				icon = value;
				if (icon != null && icon.StartsWith ("stock:")) {
					Gtk.StockItem item = Gtk.Stock.Lookup (icon.Substring (6));
					if (item.Label != null) {
						label = item.Label;
						EmitNotify ("Label");
					}
				}
				ConstructChild ();
			}
		}

		[GladeProperty (GladeProperty.UseUnderlying)]
		public string Label {
			get {
				return label;
			}
			set {
				label = value;
				ConstructChild ();
			}
		}

		public bool UseUnderline {
			get {
				return button.UseUnderline;
			}
			set {
				button.UseUnderline = value;
				ConstructChild ();
			}
		}

		public bool HasResponseId {
			get {
				WidgetSite site = button.Parent as WidgetSite;
				if (site == null)
					return false;
				site = site.ParentSite as WidgetSite;
				if (site == null)
					return false;
				return site.InternalChildId == "action_area";
			}
		}

		int responseId;

		[GladeProperty (Name = "response_id")]
		[Editor (typeof (Stetic.Editor.ResponseId))]
		[Description ("Response Id", "The response ID to emit when this button is clicked.")]
		public int ResponseId {
			get {
				return responseId;
			}
			set {
				responseId = value;
			}
		}
	}
}

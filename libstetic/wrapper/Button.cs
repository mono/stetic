using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Button", "button.png", ObjectWrapperType.Widget)]
	public class Button : Container {

		public static new Type WrappedType = typeof (Gtk.Button);

		internal static new void Register (Type type)
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

			if (button.UseStock)
				Icon = "stock:" + button.Label;
			else {
				Icon = null;
				if (initialized)
					Label = button.Label;
				else
					Label = button.Name;
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
			hasLabel = false;

			Gtk.Alignment alignment = button.Child as Gtk.Alignment;
			if (alignment == null)
				return;
			Gtk.HBox box = alignment.Child as Gtk.HBox;
			if (box == null)
				return;

			Gtk.Widget[] children = box.Children;
			if (children == null || children.Length != 2)
				return;

			Gtk.Image image = children[0] as Gtk.Image;
			Gtk.Label label = children[1] as Gtk.Label;
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
			hasLabel = true;
		}

		public override void GladeExport (out string className, out string id, out Hashtable props)
		{
			base.GladeExport (out className, out id, out props);
			props["use_stock"] = IsStock ? "True" : "False";
		}

		public override IEnumerable RealChildren {
			get {
				if (HasLabel)
					return new Gtk.Widget[0];
				else
					return base.RealChildren;
			}
		}

		public override IEnumerable GladeChildren {
			get {
				if (Icon == null || IsStock)
					return new Gtk.Widget[0];
				else
					return base.RealChildren;
			}
		}

		private Gtk.Button button {
			get {
				return (Gtk.Button)Wrapped;
			}
		}

		// true if the button has an icon+label rather than custom contents
		bool hasLabel;
		public bool HasLabel {
			get {
				return hasLabel;
			}
		}

		// true if the button has *anything* in it
		public bool HasContents {
			get {
				return !(button.Child is Placeholder);
			}
		}

		[Command ("Remove Button Contents", "Remove the current contents of the button")]
		internal void RemoveContents ()
		{
			if (button.Child != null)
				button.Remove (button.Child);

			button.Add (CreatePlaceholder ());
			hasLabel = false;

			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
		}

		[Command ("Restore Button Label", "Restore the button's label")]
		internal void RestoreLabel ()
		{
			ConstructChild ();
			EmitNotify ("HasContents");
			EmitNotify ("HasLabel");
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);
			EmitNotify ("HasContents");
		}

		Gtk.Image iconWidget;
		Gtk.Label labelWidget;
		string icon;
		string label;

		void ConstructChild ()
		{
			button.UseStock = false;
			if (button.Child != null)
				button.Remove (button.Child);
			iconWidget = null;
			labelWidget = null;
			hasLabel = true;

			bool useStock = icon != null && icon.StartsWith ("stock:");

			if (button.UseUnderline || useStock) {			    
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

			if (useStock)
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

			Widget wrapper;
			wrapper = (Widget)ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Label), labelWidget);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Image), iconWidget);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.HBox), box);
			wrapper.Unselectable = true;
			wrapper = (Widget)ObjectWrapper.Create (stetic, typeof (Stetic.Wrapper.Alignment), alignment);
			wrapper.Unselectable = true;
		}

		bool IsStock {
			get {
				if (icon == null || !icon.StartsWith ("stock:"))
					return false;

				Gtk.StockItem item = Gtk.Stock.Lookup (icon.Substring (6));
				return item.Label != null && label == item.Label;
			}
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
				Stetic.Wrapper.Widget pwrap = ParentWrapper;
				if (pwrap == null)
					return false;
				return pwrap.InternalChildId == "action_area";
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

using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Box", "checkbutton.png", ObjectWrapperType.Widget)]
	public class CheckButton : Container {

		public static new Type WrappedType = typeof (Gtk.CheckButton);

		internal static new void Register (Type type)
		{
			if (type == typeof (Stetic.Wrapper.CheckButton)) {
				AddItemGroup (type, "Check Box Properties",
					      "Label",
					      "Active",
					      "Inconsistent",
					      "DrawIndicator",
					      "RemoveLabel",
					      "RestoreLabel");
			}

			PropertyDescriptor hasLabel =
				new PropertyDescriptor (typeof (Stetic.Wrapper.CheckButton),
							typeof (Gtk.CheckButton),
							"HasLabel");

			ItemGroup props = (ItemGroup)(GetItemGroups (type)[0]);
			props["Label"].DisabledIf (hasLabel, false);
			props["RemoveLabel"].DisabledIf (hasLabel, false);
			props["RestoreLabel"].DisabledIf (hasLabel, true);

			props = AddContextMenuItems (type,
						     "RemoveLabel",
						     "RestoreLabel");
			props["RemoveLabel"].DisabledIf (hasLabel, false);
			props["RestoreLabel"].DisabledIf (hasLabel, true);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.CheckButton checkbutton = (Gtk.CheckButton)obj;
				checkbutton.Label = checkbutton.Name;
			}
		}

		public override Widget GladeImportChild (string className, string id,
							 Hashtable props, Hashtable childprops)
		{
			hasLabel = false;
			return base.GladeImportChild (className, id, props, childprops);
		}

		public Gtk.CheckButton checkbutton {
			get {
				return (Gtk.CheckButton)Wrapped;
			}
		}

		public override bool HExpandable { get { return true; } }

		bool hasLabel = true;
		public bool HasLabel {
			get {
				return hasLabel;
			}
			set {
				hasLabel = value;
				EmitNotify ("HasLabel");
			}
		}

		[Command ("Remove Label", "Remove the button's label")]
		internal void RemoveLabel ()
		{
			if (checkbutton.Child != null)
				checkbutton.Remove (checkbutton.Child);

			AddPlaceholder ();
			HasLabel = false;
		}

		[Command ("Restore Label", "Restore the button's label")]
		internal void RestoreLabel ()
		{
			checkbutton.Label = checkbutton.Name;
			HasLabel = true;
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);
			EmitNotify ("HasContents");
		}

	}
}

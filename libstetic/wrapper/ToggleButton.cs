using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", ObjectWrapperType.Widget)]
	public class ToggleButton : Button {

		public static new Type WrappedType = typeof (Gtk.ToggleButton);

		static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Toggle Button Properties",
							"Icon",
							"Label",
							"Active",
							"Inconsistent",
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

			props = AddContextMenuItems (type,
						     "RemoveContents",
						     "RestoreLabel");
			props["RemoveContents"].DependsOn (hasContents);
			props["RestoreLabel"].DependsInverselyOn (hasLabel);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Icon = null;
				Label = ((Gtk.Widget)Wrapped).Name;
			}
		}
	}
}

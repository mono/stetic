using System;
using System.Collections;
using System.Xml;
using System.CodeDom;

namespace Stetic.Wrapper {

	public class RadioToolButton : ToggleToolButton {

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.RadioToolButton (new GLib.SList (IntPtr.Zero), Gtk.Stock.SortAscending);
		}

		static RadioGroupManager GroupManager = new RadioGroupManager (typeof (Gtk.RadioToolButton));

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.RadioToolButton radio = (Gtk.RadioToolButton)Wrapped;
			if (!initialized)
				Group = GroupManager.LastGroup;
			else if (radio.Group == null)
				Group = radio.Name;
		}

		public override void Read (XmlElement elem, FileFormat format)
		{
			string group = (string)GladeUtils.ExtractProperty (elem, "group", "");
			bool active = (bool)GladeUtils.ExtractProperty (elem, "active", false);
			base.Read (elem, format);

			if (group != "")
				Group = group;
			else
				Group = Wrapped.Name;
			if (active)
				((Gtk.RadioToolButton)Wrapped).Active = true;
		}

		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			string group = GroupManager.GladeGroupName (Wrapped);
			if (group != Wrapped.Name)
				GladeUtils.SetProperty (elem, "group", group);
			return elem;
		}
		
		protected override void GeneratePropertySet (GeneratorContext ctx, CodeStatementCollection statements, CodeVariableReferenceExpression var, TypedPropertyDescriptor prop)
		{
			if (prop.Name == "Group") {
				CodeExpression groupExp = GroupManager.GenerateGroupExpression (ctx, (Gtk.Widget) Wrapped);
				statements.Add (
					new CodeAssignStatement (
						new CodePropertyReferenceExpression (var, "Group"),
						groupExp)
				);
			}
			else
				base.GeneratePropertySet (ctx, statements, var, prop);
		}

		public string Group {
			get {
				return GroupManager[Wrapped];
			}
			set {
				GroupManager[Wrapped] = value;
				EmitNotify ("Group");
			}
		}
	}
}

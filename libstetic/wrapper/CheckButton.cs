using System;
using System.Xml;
using System.CodeDom;

namespace Stetic.Wrapper {

	public class CheckButton : Container {

		protected override Widget ReadChild (XmlElement child_elem, FileFormat format)
		{
			hasLabel = false;
			if (checkbutton.Child != null)
				checkbutton.Remove (checkbutton.Child);
			return base.ReadChild (child_elem, format);
		}

		public Gtk.CheckButton checkbutton {
			get {
				return (Gtk.CheckButton)Wrapped;
			}
		}

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

		internal void RemoveLabel ()
		{
			if (checkbutton.Child != null)
				checkbutton.Remove (checkbutton.Child);

			AddPlaceholder ();
			HasLabel = false;
		}

		internal void RestoreLabel ()
		{
			checkbutton.Label = checkbutton.Name;
			HasLabel = true;
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);
			EmitNotify ("HasContents");
		}

		protected override void GenerateChildBuildCode (GeneratorContext ctx, string parentVar, Widget wrapper)
		{
			if (!HasLabel) {
				// CheckButton generates a label by default. Remove it if it is not required.
				ctx.Statements.Add (
					new CodeMethodInvokeExpression (
						new CodeVariableReferenceExpression (parentVar),
						"Remove",
						new CodePropertyReferenceExpression (
							new CodeVariableReferenceExpression (parentVar),
							"Child"
						)
					)
				);
			}
			base.GenerateChildBuildCode (ctx, parentVar, wrapper);
		}
	}
}

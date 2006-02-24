using GLib;
using System;
using System.CodeDom;
using System.Collections;

namespace Stetic.Wrapper {

	public class Window : Container {

		public override void Wrap (object obj, bool initialized)
		{
			Gtk.Window window = (Gtk.Window)obj;

			window.TypeHint = Gdk.WindowTypeHint.Normal;
			base.Wrap (obj, initialized);

			if (!initialized) {
				if (window.Child is Placeholder)
					window.Child.SetSizeRequest (200, 200);
			}

			window.DeleteEvent += DeleteEvent;
		}

		[ConnectBefore]
		void DeleteEvent (object obj, Gtk.DeleteEventArgs args)
		{
			Wrapped.Hide ();
			args.RetVal = true;
		}

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }

		// We don't want to actually set the underlying properties for these;
		// that would be annoying to interact with.
		bool modal;
		public bool Modal {
			get {
				return modal;
			}
			set {
				modal = value;
				EmitNotify ("Modal");
			}
		}

		Gdk.WindowTypeHint typeHint;
		public Gdk.WindowTypeHint TypeHint {
			get {
				return typeHint;
			}
			set {
				typeHint = value;
				EmitNotify ("TypeHint");
			}
		}

		Gtk.WindowType type;
		public Gtk.WindowType Type {
			get {
				return type;
			}
			set {
				type = value;
				EmitNotify ("Type");
			}
		}

		string icon;
		public string Icon {
			get {
				return icon;
			}
			set {
				icon = value;
				Gtk.Window window = (Gtk.Window)Wrapped;
				try {
					window.Icon = new Gdk.Pixbuf (icon);
				} catch {
					window.Icon = null;
				}
			}
		}
		
		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName, CodeStatementCollection statements)
		{
			base.GenerateBuildCode (ctx, varName, statements);
			if (((Gtk.Window)Wrapped).DefaultWidth == -1) {
				statements.Add (
					new CodeAssignStatement (
						new CodePropertyReferenceExpression (
							new CodeVariableReferenceExpression (varName),
							"DefaultWidth"
						),
						new CodePrimitiveExpression (DesignWidth)
					)
				);
			}
				
			if (((Gtk.Window)Wrapped).DefaultHeight == -1) {
				statements.Add (
					new CodeAssignStatement	 (
						new CodePropertyReferenceExpression (
							new CodeVariableReferenceExpression (varName),
							"DefaultHeight"
						),
						new CodePrimitiveExpression (DesignHeight)
					)
				);
			}
		}
	}
}

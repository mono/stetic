using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public class Grid : Gtk.Container {

		public Grid () : base ()
		{
			BorderWidth = 2;
			WidgetFlags |= WidgetFlags.NoWindow;
			groups = new ArrayList ();
		}

		const int groupPad = 6;
		const int hPad = 6;
		const int linePad = 3;

		protected ArrayList groups;

		public class Group {
			Stetic.Grid parent;
			Gtk.Expander expander;
			ArrayList names, editors;

			public Group (Grid parent, string name) {
				this.parent = parent;

				expander = new Gtk.Expander ("<b>" + name + "</b>");
				expander.UseMarkup = true;
				expander.Show ();
				expander.AddNotification ("expanded", Expanded);

				names = new ArrayList ();
				editors = new ArrayList ();
			}

			void Expanded (object o, GLib.NotifyArgs args)
			{
				foreach (Widget w in names) {
					if (expander.Expanded)
						w.Show ();
					else
						w.Hide ();
				}
				foreach (Widget w in editors) {
					if (expander.Expanded)
						w.Show ();
					else
						w.Hide ();
				}

				parent.QueueDraw ();
			}

			public void Add (string name, Widget editor)
			{
				Label label = new Label (name);
				label.UseMarkup = true;
				label.Justify = Justification.Left;
				label.Xalign = 0;
				label.Parent = parent;
				label.Show ();
				names.Add (label);

				editor.Parent = parent;
				editors.Add (editor);

				parent.QueueDraw ();
			}

			public Gtk.Expander Expander {
				get {
					return expander;
				}
			}

			public ArrayList Names {
				get {
					return names;
				}
			}

			public ArrayList Editors {
				get {
					return editors;
				}
			}
		}

		public Group AddGroup (string name, bool expanded)
		{
			Group g = new Group (this, name);
			groups.Add (g);
			g.Expander.Parent = this;
			g.Expander.Expanded = expanded;
			QueueDraw ();
			return g;
		}

		public void RemoveGroup (Group g)
		{
			foreach (Label l in g.Names)
				l.Unparent ();
			foreach (Widget e in g.Editors)
				e.Unparent ();
			g.Expander.Unparent ();
			groups.Remove (g);
			QueueDraw ();
		}

		protected void Clear ()
		{
			while (groups.Count > 0)
				RemoveGroup (groups[0] as Group);
		}

		protected override void ForAll (bool include_internals, CallbackInvoker invoker)
		{
			if (!include_internals)
				return;

			foreach (Group g in groups) {
				invoker.Invoke (g.Expander);
				for (int i = 0; i < g.Names.Count; i++) {
					invoker.Invoke (g.Names[i] as Widget);
					invoker.Invoke (g.Editors[i] as Widget);
				}
			}
		}

		// These are figured out at requisition time and used again at
		// allocation time.
		int nwidth, ewidth;
		int indent, lineHeight;

		protected override void OnSizeRequested (ref Gtk.Requisition req)
		{
			if (groups.Count == 0) {
				req.Height = 2 * (int)BorderWidth;
				req.Width = 2 * (int)BorderWidth;
				return;
			}

			req.Width = req.Height = 0;
			nwidth = ewidth = 0;

			// Requisition the expanders themselves
			Gtk.Requisition exreq, lreq;
			foreach (Group group in groups) {
				exreq = group.Expander.SizeRequest ();
				req.Height += exreq.Height;
				if (req.Width < exreq.Width)
					req.Width = exreq.Width;
			}
			req.Height += (groups.Count - 1) * groupPad;

			// Figure out the indent and lineHeight from the first expander.
			// (Seems like there should be an easier way to find the indent...)
			Expander exp = ((Group)groups[0]).Expander;

			int focusWidth = (int)exp.StyleGetProperty ("focus-line-width");
			int focusPad = (int)exp.StyleGetProperty ("focus-padding");
			int expanderSize = (int)exp.StyleGetProperty ("expander-size");
			int expanderSpacing = (int)exp.StyleGetProperty ("expander-spacing");
			indent = (int)exp.BorderWidth + focusWidth + focusPad +
				expanderSize + 2 * expanderSpacing;

			lreq = ((Group)groups[0]).Expander.LabelWidget.ChildRequisition;
			lineHeight = (int)(1.5 * lreq.Height);

			// Now requisition the contents of the groups. (We have to do
			// all of them, even if they're not expanded, so that the
			// column widths don't change when groups are expanded or
			// contracted.)
			foreach (Group group in groups) {
				for (int i = 0; i < group.Names.Count; i++) {
					Gtk.Widget name = group.Names[i] as Widget;
					Gtk.Widget editor = group.Editors[i] as Widget;
					Gtk.Requisition nreq, ereq;

					nreq = name.SizeRequest ();
					ereq = editor.SizeRequest ();

					if (nreq.Width > nwidth)
						nwidth = nreq.Width;
					if (ereq.Width > ewidth)
						ewidth = ereq.Width;

					if (group.Expander.Expanded)
						req.Height += Math.Max (lineHeight, ereq.Height) + linePad;
				}
			}

			req.Width = Math.Max (req.Width, indent + nwidth + hPad + ewidth);

			req.Height += 2 * (int)BorderWidth;
			req.Width += 2 * (int)BorderWidth;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle alloc)
		{
			int xbase = alloc.X + (int)BorderWidth;
			int ybase = alloc.Y + (int)BorderWidth;

			base.OnSizeAllocated (alloc);

			int y = ybase;

			foreach (Group group in groups) {
				Gdk.Rectangle exalloc;
				Gtk.Requisition exreq;

				exreq = group.Expander.ChildRequisition;
				exalloc.X = xbase;
				exalloc.Y = y;
				exalloc.Width = exreq.Width;
				exalloc.Height = exreq.Height;
				group.Expander.SizeAllocate (exalloc);

				y += exalloc.Height;

				if (group.Expander.Expanded) {
					for (int i = 0; i < group.Names.Count; i++) {
						Gtk.Widget name = group.Names[i] as Widget;
						Gtk.Widget editor = group.Editors[i] as Widget;
						Gtk.Requisition nreq, ereq;
						Gdk.Rectangle nalloc, ealloc;

						nreq = name.ChildRequisition;
						ereq = editor.ChildRequisition;

						nalloc.X = xbase + indent;
						nalloc.Y = y + (lineHeight - nreq.Height) / 2;
						nalloc.Width = nwidth;
						nalloc.Height = nreq.Height;
						name.SizeAllocate (nalloc);

						ealloc.X = nalloc.X + nwidth + hPad;
						ealloc.Y = y + Math.Max (0, (lineHeight - ereq.Height) / 2);
						ealloc.Width = Math.Max (ewidth, alloc.Width - 2 * (int)BorderWidth - ealloc.X);
						ealloc.Height = ereq.Height;
						editor.SizeAllocate (ealloc);

						y += Math.Max (ereq.Height, lineHeight) + linePad;
					}
				}

				y += groupPad;
			}
		}
	}
}

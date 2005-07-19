using Gtk;
using System;

namespace Stetic {

	public class UIManager : Gtk.UIManager {

		static string menuXml =
		"<ui>" +
		"    <menubar>" +
		"	<menu action='FileMenu'>" +
		"	    <menuitem action='ImportGlade' />" +
		"	    <menuitem action='ExportGlade' />" +
		"	    <separator />" +
		"	    <menuitem action='Quit' />" +
		"	</menu>" +
		"	<menu action='EditMenu'>" +
		"	    <menuitem action='Undo' />" +
		"	    <menuitem action='Redo' />" +
		"	    <separator />" +
		"	    <menuitem action='Cut' />" +
		"	    <menuitem action='Copy' />" +
		"	    <menuitem action='Paste' />" +
		"	    <menuitem action='Delete' />" +
		"	</menu>" +
		"	<menu action='HelpMenu'>" +
		"	    <menuitem action='Contents' />" +
		"	    <menuitem action='About' />" +
		"	</menu>" +
		"    </menubar>" +
		"</ui>";

		public UIManager (Project project)
		{
			ActionEntry[] entries = new ActionEntry[] {
				new ActionEntry ("FileMenu", null, "_File", null, null, null),
				new ActionEntry ("ImportGlade", Stock.Open, "_Import from Glade File...", null, "Import UI from a Glade file", ImportGlade),
				new ActionEntry ("ExportGlade", Stock.SaveAs, "_Export to Glade File...", null, "Export UI to a Glade file", ExportGlade),
				new ActionEntry ("Quit", Stock.Quit, null, "<control>Q", "Quit", Quit),

				new ActionEntry ("EditMenu", null, "_Edit", null, null, null),
				new ActionEntry ("Undo", Stock.Undo, null, "<control>Z", "Undo previous action", Undo),
				new ActionEntry ("Redo", Stock.Redo, null, "<control><shift>Z", "Redo previously-undone action", Redo),
				new ActionEntry ("Cut", Stock.Cut, null, "<control>X", "Cut selection to clipboard", Cut),
				new ActionEntry ("Copy", Stock.Copy, null, "<control>C", "Copy selection to clipboard", Copy),
				new ActionEntry ("Paste", Stock.Paste, null, "<control>V", "Paste from clipboard", Paste),
				new ActionEntry ("Delete", Stock.Delete, null, "Delete", "Delete selection", Delete),

				new ActionEntry ("HelpMenu", null, "_Help", null, null, null),
				new ActionEntry ("Contents", Stock.Help, "_Contents", "F1", "Help", Help),
				new ActionEntry ("About", Stock.About, null, null, "About Stetic", About),
			};

			ActionGroup actions = new ActionGroup ("group");
			actions.Add (entries);

			InsertActionGroup (actions, 0);
			AddUiFromString (menuXml);

			// Not yet implemented
			GetAction ("/menubar/EditMenu/Undo").Sensitive = false;
			GetAction ("/menubar/EditMenu/Redo").Sensitive = false;
			GetAction ("/menubar/HelpMenu/Contents").Sensitive = false;

			// Set up Edit menu sensitivity hackery
			Gtk.MenuItem editMenuItem = (Gtk.MenuItem) GetWidget ("/menubar/EditMenu");
			Gtk.Menu editMenu = (Gtk.Menu)editMenuItem.Submenu;
			editMenu.Shown += EditShown;
			editMenu.Hidden += EditHidden;

			Project = project;
		}

		Project project;
		Project Project {
			get {
				return project;
			}
			set {
				if (project != null)
					project.Selected -= Selected;

				project = value;

				if (project != null) {
					project.Selected += Selected;
					Selected (project.Selection, null);
				} else
					Selected (null, null);

				GetAction ("/menubar/FileMenu/ImportGlade").Sensitive = (project != null);
				GetAction ("/menubar/FileMenu/ExportGlade").Sensitive = (project != null);
			}
		}

		void ImportGlade (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Import from Glade File", null, FileChooserAction.Open,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				GladeFiles.Import (project, dialog.Filename);
			dialog.Hide ();
		}

		void ExportGlade (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Export to Glade File", null, FileChooserAction.Save,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Save, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				GladeFiles.Export (project, dialog.Filename);
			dialog.Hide ();
		}

		void Undo (object obj, EventArgs e)
		{
			Console.WriteLine ("Undo");
		}

		void Redo (object obj, EventArgs e)
		{
			Console.WriteLine ("Redo");
		}

		public void Cut (Gtk.Widget widget)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			Console.WriteLine ("Cut {0}", wrapper == null ? widget.ToString () : wrapper.ToString ());
		}

		void Cut (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.CutClipboard ();
			else if (project.Selection != null)
				Cut (project.Selection);
		}

		public void Copy (Gtk.Widget widget)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			Console.WriteLine ("Copy {0}", wrapper == null ? widget.ToString () : wrapper.ToString ());
		}

		void Copy (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.CopyClipboard ();
			else if (project.Selection != null)
				Copy (project.Selection);
		}

		public void Paste (Gtk.Widget widget)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			Console.WriteLine ("Paste into {0}", wrapper == null ? widget.ToString () : wrapper.ToString ());
		}

		void Paste (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.PasteClipboard ();
			else if (project.Selection != null)
				Paste (project.Selection);
		}

		public void Delete (Gtk.Widget widget)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			if (wrapper != null)
				wrapper.Delete ();
		}

		void Delete (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.DeleteSelection ();
			else if (project.Selection != null)
				Delete (project.Selection);
		}

		void Help (object obj, EventArgs e)
		{
			Console.WriteLine ("Help");
		}

		void About (object obj, EventArgs e)
		{
			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.Name = "Stetic";
			about.Version = "0.0.0";
			about.Comments = "A GNOME and Gtk GUI designer";
			about.Authors = new string[] { "Dan Winship" };
			about.Copyright = "Copyright 2004, 2005 Novell, Inc.";
			about.Website = "http://mono-project.com/Stetic";
			about.Run ();
		}

		void Quit (object obj, EventArgs e)
		{
			SteticMain.Program.Quit ();
		}

		public Gtk.MenuBar MenuBar {
			get {
				return (Gtk.MenuBar) GetWidget ("/menubar");
			}
		}

		Gtk.Editable Focus {
			get {
				if (SteticMain.MainWindow.HasToplevelFocus)
					return SteticMain.MainWindow.Focus as Gtk.Editable;
				else
					return null;
			}
		}

		void EditShown (object obj, EventArgs args)
		{
			Gtk.Editable focus = Focus;
			if (focus != null) {
				int selStart, selEnd;
				bool hasSelection, editable;
				hasSelection = focus.GetSelectionBounds (out selStart, out selEnd);
				editable = focus.IsEditable;
				UpdateEdit (hasSelection && editable, hasSelection, editable);
			} else
				UpdateEdit (project.Selection);
		}

		void EditHidden (object obj, EventArgs args)
		{
			UpdateEdit (true, true, true);
		}

		public void UpdateEdit (bool cut, bool copy, bool paste)
		{
			GetAction ("/menubar/EditMenu/Cut").Sensitive = cut;
			GetAction ("/menubar/EditMenu/Copy").Sensitive = copy;
			GetAction ("/menubar/EditMenu/Paste").Sensitive = paste;
			GetAction ("/menubar/EditMenu/Delete").Sensitive = cut;
		}

		void UpdateEdit (Gtk.Widget selection)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (selection);
			if (wrapper != null)
				UpdateEdit (wrapper.InternalChildId == null, true, false);
			else if (selection is Placeholder)
				UpdateEdit (false, false, true);
			else
				UpdateEdit (false, false, false);
		}

		void Selected (Gtk.Widget selection, ProjectNode node)
		{
			UpdateEdit (selection);
		}
	}
}

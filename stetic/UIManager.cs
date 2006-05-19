using Gtk;
using System;

namespace Stetic {

	public class UIManager : Gtk.UIManager {

		static string menuXml =
		"<ui>" +
		"    <menubar>" +
		"	<menu action='FileMenu'>" +
		"	    <menuitem action='Open' />" +
		"	    <separator />" +
		"	    <menuitem action='Save' />" +
		"	    <menuitem action='SaveAs' />" +
		"	    <separator />" +
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
		"	<menu action='ProjectMenu'>" +
		"	    <menuitem action='EditProjectIcons' />" +
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
				new ActionEntry ("Open", Stock.Open, null, "<control>O", "Open a file", OpenFile),
				new ActionEntry ("Save", Stock.Save, null, "<control>S", "Save", SaveFile),
				new ActionEntry ("SaveAs", Stock.SaveAs, null, "<control><shift>S", "Save As", SaveFileAs),
				new ActionEntry ("ImportGlade", null, "_Import from Glade File...", null, "Import UI from a Glade file", ImportGlade),
				new ActionEntry ("ExportGlade", null, "_Export to Glade File...", null, "Export UI to a Glade file", ExportGlade),
				new ActionEntry ("Quit", Stock.Quit, null, "<control>Q", "Quit", Quit),

				new ActionEntry ("EditMenu", null, "_Edit", null, null, null),
				new ActionEntry ("Undo", Stock.Undo, null, "<control>Z", "Undo previous action", Undo),
				new ActionEntry ("Redo", Stock.Redo, null, "<control><shift>Z", "Redo previously-undone action", Redo),
				new ActionEntry ("Cut", Stock.Cut, null, "<control>X", "Cut selection to clipboard", Cut),
				new ActionEntry ("Copy", Stock.Copy, null, "<control>C", "Copy selection to clipboard", Copy),
				new ActionEntry ("Paste", Stock.Paste, null, "<control>V", "Paste from clipboard", Paste),
				new ActionEntry ("Delete", Stock.Delete, null, "Delete", "Delete selection", Delete),

				new ActionEntry ("ProjectMenu", null, "Project", null, null, null),
				new ActionEntry ("EditProjectIcons", null, "Project Icons...", null, null, EditIcons),

				new ActionEntry ("HelpMenu", null, "_Help", null, null, null),
				new ActionEntry ("Contents", Stock.Help, "_Contents", "F1", "Help", Help),
				new ActionEntry ("About", Gnome.Stock.About, null, null, "About Stetic", About),
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
					project.SelectionChanged -= Selected;

				project = value;

				if (project != null) {
					project.SelectionChanged += Selected;
					UpdateEdit (project.Selection);
				} else
					UpdateEdit (null);

				GetAction ("/menubar/FileMenu/ImportGlade").Sensitive = (project != null);
				GetAction ("/menubar/FileMenu/ExportGlade").Sensitive = (project != null);
			}
		}

		void OpenFile (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Open Stetic File", null, FileChooserAction.Open,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				project.Load (dialog.Filename);
			dialog.Hide ();
		}
		
		void SaveFile (object obj, EventArgs e)
		{
			if (project.FileName == null)
				SaveFileAs (obj, e);
			else
				project.Save (project.FileName);
		}
		
		void SaveFileAs (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Save Stetic File As", null, FileChooserAction.Save,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Save, Gtk.ResponseType.Ok);

			if (project.FileName != null)
				dialog.CurrentName = project.FileName;

			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok) {
				string name = dialog.Filename;
				if (System.IO.Path.GetExtension (name) == "")
					name = name + ".stetic";
				project.Save (name);
			}
			dialog.Hide ();
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

		void Cut (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.CutClipboard ();
			else if (project.Selection != null)
				Clipboard.Cut (project.Selection);
		}

		void Copy (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.CopyClipboard ();
			else if (project.Selection != null)
				Clipboard.Copy (project.Selection);
		}

		void Paste (object obj, EventArgs e)
		{
			Gtk.Editable focus = Focus;
			if (focus != null)
				focus.PasteClipboard ();
			else if (project.Selection != null)
				Clipboard.Paste (project.Selection as Placeholder);
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

		void EditIcons (object obj, EventArgs e)
		{
			using (Stetic.Editor.EditIconFactoryDialog dlg = new Stetic.Editor.EditIconFactoryDialog (null, project, project.IconFactory)) {
				dlg.Run ();
			}
		}
		
		const string AppName = "Stetic";
		const string AppVersion = "0.0.0";
		const string AppComments = "A GNOME and Gtk GUI designer";
		static string[] AppAuthors = new string[] { "Dan Winship" };
		const string AppCopyright = "Copyright 2004, 2005 Novell, Inc.";

		void About (object obj, EventArgs e)
		{
#if GTK_SHARP_2_6
			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.Name = AppName;
			about.Version = AppVersion;
			about.Comments = AppComments;
			about.Authors = AppAuthors;
			about.Copyright = AppCopyright;
			about.Website = "http://mono-project.com/Stetic";
#else
			Gnome.About about = new Gnome.About (AppName, AppVersion, AppCopyright,
							     AppComments, AppAuthors,
							     new string[0],
							     null, null);
#endif
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
				UpdateEdit (wrapper.InternalChildProperty == null, true, false);
			else if (selection is Placeholder)
				UpdateEdit (false, false, true);
			else
				UpdateEdit (false, false, false);
		}

		void Selected (object s, Wrapper.WidgetEventArgs args)
		{
			UpdateEdit (args.Widget != null ? args.Widget.Wrapped : null);
		}
	}
}

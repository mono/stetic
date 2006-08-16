
using System;
using Mono.Unix;

namespace Stetic.Editor
{
	public class StockIconSelectorItem: IconSelectorItem
	{
		public StockIconSelectorItem (): base (Catalog.GetString ("Stock Icons"))
		{
		}
		
		protected override void CreateIcons ()
		{
			foreach (string s in StockIconHelper.StockIcons) {
				if (s != "-" && s != "|")
					AddIcon (s, RenderIcon (s, Gtk.IconSize.Menu, ""), s);
				else
					AddSeparator (s);
			}
		}
	}
	
	class StockIconHelper
	{
		public static string[] StockIcons = {
			// Commands
			"gtk-new",
			"gtk-open",
			"gtk-save",
			"gtk-save-as",
			"gtk-revert-to-saved",
			"gtk-quit",
			"gtk-print",
			"gtk-print-preview",
			"gtk-properties",
			"|",
			"gtk-cut",
			"gtk-copy",
			"gtk-paste",
			"gtk-delete",
			"gtk-undelete",
			"gtk-undo",
			"gtk-redo",
			"gtk-preferences",
			"|",
			"gtk-execute",
			"gtk-stop",
			"gtk-refresh",
			"gtk-find",
			"gtk-find-and-replace",
			"|",
			"gtk-spell-check",
			"gnome-stock-attach",
			"gtk-convert",
			"gtk-help",
			"gnome-stock-about",
			"|",
			"gtk-add",
			"gtk-remove",
			"gtk-clear",
			"-",

			// Formatting
			"gtk-bold",
			"gtk-italic",
			"gtk-underline",
			"gtk-strikethrough",
			"gtk-select-color",
			"gtk-select-font",
			"|",
			"gtk-indent",
			"gtk-unindent",
			"gtk-justify-center",
			"gtk-justify-fill",
			"gtk-justify-left",
			"gtk-justify-right",
			"|",
			"gtk-sort-ascending",
			"gtk-sort-descending",
			"|",
			"gnome-stock-table-borders",
			"gnome-stock-table-fill",
			"gnome-stock-text-bulleted-list",
			"gnome-stock-text-numbered-list",
			"gnome-stock-text-indent",
			"gnome-stock-text-unindent",
			"|",
			"gtk-zoom-100",
			"gtk-zoom-fit",
			"gtk-zoom-in",
			"gtk-zoom-out",
			"-",


			// Dialog
			"gtk-yes",
			"gtk-no",
			"gtk-cancel",
			"gtk-ok",
			"gtk-apply",
			"gtk-close",
			"|",
			"gtk-dialog-error",
			"gtk-dialog-info",
			"gtk-dialog-question",
			"gtk-dialog-warning",
			"-",

			// Navigation
			"gtk-goto-bottom",
			"gtk-goto-first",
			"gtk-goto-last",
			"gtk-goto-top",
			"|",
			"gtk-go-back",
			"gtk-go-down",
			"gtk-go-forward",
			"gtk-go-up",
			"|",
			"gtk-home",
			"gtk-jump-to",
			"-",

			// Books
			"gnome-stock-book-blue",
			"gnome-stock-book-green",
			"gnome-stock-book-open",
			"gnome-stock-book-red",
			"gnome-stock-book-yellow",
			"|",
			"gnome-stock-mail",
			"gnome-stock-mail-fwd",
			"gnome-stock-mail-new",
			"gnome-stock-mail-rcv",
			"gnome-stock-mail-rpl",
			"gnome-stock-mail-snd",
			"-",		

			// Misc
			"gtk-cdrom",
			"gtk-floppy",
			"gtk-harddisk",
			"gtk-network",
			"gnome-stock-line-in",
			"gnome-stock-mic",
			"gnome-stock-midi",
			"gnome-stock-timer",
			"gnome-stock-timer-stop",
			"gnome-stock-volume",
			"gnome-stock-trash",
			"gnome-stock-trash-full",
			"gtk-color-picker",
			"gtk-dnd",
			"gtk-dnd-multiple",
			"gtk-missing-image",
			"gtk-index",
			"gnome-stock-authentication",
			"gnome-stock-blank",
			"gnome-stock-multiple-file",
			"gnome-stock-not",
			"gnome-stock-scores",
			};
	}
}

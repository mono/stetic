SUBDIRS = pixmaps

noinst_DATA = libsteticwidgets.dll

libsteticwidgets_dll_sources =		\
	Arrow.cs			\
	Box.cs				\
	Button.cs			\
	ButtonBox.cs			\
	CheckButton.cs			\
	ColorButton.cs			\
	ComboBox.cs			\
	Dialog.cs			\
	DrawingArea.cs			\
	Entry.cs			\
	Frame.cs			\
	HBox.cs				\
	HButtonBox.cs			\
	HPaned.cs			\
	HScale.cs			\
	HScrollbar.cs			\
	HSeparator.cs			\
	Icon.cs				\
	Image.cs			\
	Label.cs			\
	Misc.cs				\
	Notebook.cs			\
	Paned.cs			\
	ProgressBar.cs			\
	RadioButton.cs			\
	Range.cs			\
	Scale.cs			\
	SpinButton.cs			\
	Statusbar.cs			\
	Table.cs			\
	TextView.cs			\
	ToggleButton.cs			\
	VBox.cs				\
	VButtonBox.cs			\
	VPaned.cs			\
	VScale.cs			\
	VScrollbar.cs			\
	VSeparator.cs			\
	Widget.cs			\
	Window.cs

comma = ,
ICON_FILES = $(wildcard $(srcdir)/pixmaps/*.png)
ICON_RESOURCES = $(foreach file,$(ICON_FILES),-resource:$(file)$(comma)$(notdir $(file)))

libsteticwidgets.dll: $(libsteticwidgets_dll_sources) $(ICON_FILES) ../libstetic/libstetic.dll
	$(MCS) -g -pkg:gnome-sharp-2.0 -r:../libstetic/libstetic.dll -target:library -o $@ $(libsteticwidgets_dll_sources) $(ICON_RESOURCES)

CLEANFILES = libsteticwidgets.dll libsteticwidgets.dll.mdb

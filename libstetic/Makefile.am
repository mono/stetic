noinst_DATA = libstetic.dll

libstetic_dll_sources =			\
	CommandAttribute.cs		\
	CommandDescriptor.cs		\
	ItemDescriptor.cs		\
	ItemGroup.cs			\
	IStetic.cs			\
	IWidgetSite.cs			\
	ObjectWrapperAttribute.cs	\
	ParamFlags.cs			\
	ParamSpec.cs			\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	SiteContentEnumerator.cs	\
	WidgetBox.cs			\
	WidgetSite.cs			\
	editor/Boolean.cs		\
	editor/Char.cs			\
	editor/Color.cs			\
	editor/Enumeration.cs		\
	editor/File.cs			\
	editor/FloatRange.cs		\
	editor/IntRange.cs		\
	editor/OptIntRange.cs		\
	editor/StockItem.cs		\
	editor/String.cs		\
	editor/Text.cs			\
	wrapper/Alignment.cs		\
	wrapper/Arrow.cs		\
	wrapper/Bin.cs			\
	wrapper/Box.cs			\
	wrapper/Button.cs		\
	wrapper/ButtonBox.cs		\
	wrapper/Calendar.cs		\
	wrapper/CheckButton.cs		\
	wrapper/ColorButton.cs		\
	wrapper/ComboBox.cs		\
	wrapper/ComboBoxEntry.cs	\
	wrapper/Container.cs		\
	wrapper/Dialog.cs		\
	wrapper/DrawingArea.cs		\
	wrapper/Entry.cs		\
	wrapper/EventBox.cs		\
	wrapper/Expander.cs		\
	wrapper/FontButton.cs		\
	wrapper/Frame.cs		\
	wrapper/HBox.cs			\
	wrapper/HButtonBox.cs		\
	wrapper/HPaned.cs		\
	wrapper/HScale.cs		\
	wrapper/HScrollbar.cs		\
	wrapper/HSeparator.cs		\
	wrapper/Icon.cs			\
	wrapper/Image.cs		\
	wrapper/Label.cs		\
	wrapper/MessageDialog.cs	\
	wrapper/Misc.cs			\
	wrapper/Notebook.cs		\
	wrapper/Object.cs		\
	wrapper/Paned.cs		\
	wrapper/ProgressBar.cs		\
	wrapper/RadioButton.cs		\
	wrapper/Range.cs		\
	wrapper/Scale.cs		\
	wrapper/Scrollbar.cs		\
	wrapper/ScrolledWindow.cs	\
	wrapper/SpinButton.cs		\
	wrapper/Statusbar.cs		\
	wrapper/Table.cs		\
	wrapper/TextView.cs		\
	wrapper/ToggleButton.cs		\
	wrapper/VBox.cs			\
	wrapper/VButtonBox.cs		\
	wrapper/VPaned.cs		\
	wrapper/VScale.cs		\
	wrapper/VScrollbar.cs		\
	wrapper/VSeparator.cs		\
	wrapper/Widget.cs		\
	wrapper/Window.cs

comma = ,
ICON_FILES = $(wildcard $(srcdir)/wrapper/pixmaps/*.png)
ICON_RESOURCES = $(foreach file,$(ICON_FILES),-resource:$(file)$(comma)$(notdir $(file)))

libstetic.dll: $(libstetic_dll_sources) $(ICON_FILES)
	$(MCS) -g -pkg:gnome-sharp-2.0 -target:library -o $@ $(libstetic_dll_sources) $(ICON_RESOURCES)

CLEANFILES = libstetic.dll libstetic.dll.mdb

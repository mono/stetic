noinst_PROGRAMS = stetic.exe
noinst_SCRIPTS = stetic

stetic_exe_SOURCES =			\
	ContextMenu.cs			\
	IObjectWrapper.cs		\
	Notify.cs			\
	Palette.cs			\
	ParamFlags.cs			\
	ParamSpec.cs			\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	PropertyEditor.cs		\
	PropertyGrid.cs			\
	Stetic.cs			\
	WidgetBox.cs			\
	WidgetFactory.cs		\
	WidgetSite.cs			\
	WidgetWrapperAttribute.cs	\
	editor/Boolean.cs		\
	editor/Char.cs			\
	editor/Enumeration.cs		\
	editor/File.cs			\
	editor/FloatRange.cs		\
	editor/IntRange.cs		\
	editor/OptIntRange.cs		\
	editor/StockItem.cs		\
	editor/String.cs		\
	wrapper/Arrow.cs		\
	wrapper/Box.cs			\
	wrapper/Button.cs		\
	wrapper/ButtonBox.cs		\
	wrapper/CheckButton.cs		\
	wrapper/ColorButton.cs		\
	wrapper/ComboBox.cs		\
	wrapper/Dialog.cs		\
	wrapper/DrawingArea.cs		\
	wrapper/Entry.cs		\
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
	wrapper/Misc.cs			\
	wrapper/Notebook.cs		\
	wrapper/Paned.cs		\
	wrapper/ProgressBar.cs		\
	wrapper/RadioButton.cs		\
	wrapper/Range.cs		\
	wrapper/Scale.cs		\
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
STETIC_RESOURCE_FILES = $(wildcard $(top_srcdir)/pixmaps/*.png)
STETIC_RESOURCES = $(foreach file,$(STETIC_RESOURCE_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_SOURCES) $(STETIC_RESOURCE_FILES)
	$(MCS) -g -pkg:gtk-sharp-2.0 -pkg:gnome-sharp-2.0 -o stetic.exe $(stetic_exe_SOURCES) $(STETIC_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" $< > $@
	chmod a+x $@

CLEANFILES = stetic stetic.exe.mdb

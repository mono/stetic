pkglib_DATA = libstetic.dll libstetic.dll.config

libstetic_dll_sources =			\
	CommandAttribute.cs		\
	CommandDescriptor.cs		\
	DescriptionAttribute.cs		\
	DND.cs				\
	EditorAttribute.cs		\
	GladeException.cs		\
	GladePropertyAttribute.cs	\
	GladeUtils.cs			\
	HandleWindow.cs			\
	ItemDescriptor.cs		\
	ItemGroup.cs			\
	IStetic.cs			\
	ObjectWrapper.cs		\
	ObjectWrapperAttribute.cs	\
	ObjectWrapperType.cs		\
	ParamSpec.cs			\
	Placeholder.cs			\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	RadioGroupManager.cs		\
	RangeAttribute.cs		\
	Set.cs				\
	TranslatableAttribute.cs	\
	editor/Accelerator.cs		\
	editor/Boolean.cs		\
	editor/Char.cs			\
	editor/Color.cs			\
	editor/Enumeration.cs		\
	editor/GroupPicker.cs		\
	editor/FloatRange.cs		\
	editor/Image.cs			\
	editor/ImageFile.cs		\
	editor/IntRange.cs		\
	editor/OptIntRange.cs		\
	editor/ResponseId.cs		\
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
	wrapper/CheckMenuItem.cs	\
	wrapper/ColorButton.cs		\
	wrapper/ComboBox.cs		\
	wrapper/ComboBoxEntry.cs	\
	wrapper/Container.cs		\
	wrapper/Custom.cs		\
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
	wrapper/Image.cs		\
	wrapper/ImageMenuItem.cs	\
	wrapper/Label.cs		\
	wrapper/MessageDialog.cs	\
	wrapper/Menu.cs			\
	wrapper/MenuBar.cs		\
	wrapper/MenuItem.cs		\
	wrapper/Misc.cs			\
	wrapper/Notebook.cs		\
	wrapper/Object.cs		\
	wrapper/OptionMenu.cs		\
	wrapper/Paned.cs		\
	wrapper/ProgressBar.cs		\
	wrapper/RadioButton.cs		\
	wrapper/RadioMenuItem.cs	\
	wrapper/RadioToolButton.cs	\
	wrapper/Range.cs		\
	wrapper/Scale.cs		\
	wrapper/ScrolledWindow.cs	\
	wrapper/SeparatorMenuItem.cs	\
	wrapper/SeparatorToolItem.cs	\
	wrapper/SpinButton.cs		\
	wrapper/Statusbar.cs		\
	wrapper/Table.cs		\
	wrapper/TextView.cs		\
	wrapper/ToggleButton.cs		\
	wrapper/ToggleToolButton.cs	\
	wrapper/Toolbar.cs		\
	wrapper/ToolButton.cs		\
	wrapper/TreeView.cs		\
	wrapper/VBox.cs			\
	wrapper/VButtonBox.cs		\
	wrapper/Viewport.cs		\
	wrapper/VPaned.cs		\
	wrapper/VScale.cs		\
	wrapper/VScrollbar.cs		\
	wrapper/VSeparator.cs		\
	wrapper/Widget.cs		\
	wrapper/Window.cs

libstetic.dll: $(libstetic_dll_sources)
	@rm -f sources.rsp
	@echo $(libstetic_dll_sources) > sources.rsp
	@for pixmap in $(srcdir)/wrapper/pixmaps/*.png; do echo -resource:$$pixmap,`basename $$pixmap` >> sources.rsp; done
	$(MCS) -debug -pkg:gnome-sharp-2.0 -target:library -out:$@ @sources.rsp

CLEANFILES = libstetic.dll libstetic.dll.mdb sources.rsp

EXTRA_DIST =				\
	$(libstetic_dll_sources)	\
	libstetic.dll.config		\
	wrapper/pixmaps/COPIED		\
	wrapper/pixmaps/*.png

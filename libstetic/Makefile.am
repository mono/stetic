pkglib_DATA = libstetic.dll libstetic.dll.config

libstetic_dll_sources =			\
	ClassDescriptor.cs		\
	CommandDescriptor.cs		\
	DND.cs				\
	EditorAttribute.cs		\
	GladeException.cs		\
	GladeUtils.cs			\
	HandleWindow.cs			\
	ItemDescriptor.cs		\
	ItemGroup.cs			\
	IStetic.cs			\
	ObjectWrapper.cs		\
	ParamSpec.cs			\
	Placeholder.cs			\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	RadioGroupManager.cs		\
	Registry.cs			\
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
	editor/StringArray.cs		\
	editor/Text.cs			\
	editor/ThemedIcon.cs		\
	wrapper/AboutDialog.cs		\
	wrapper/Box.cs			\
	wrapper/Button.cs		\
	wrapper/CheckButton.cs		\
	wrapper/ColorButton.cs		\
	wrapper/ComboBox.cs		\
	wrapper/ComboBoxEntry.cs	\
	wrapper/Container.cs		\
	wrapper/Custom.cs		\
	wrapper/Dialog.cs		\
	wrapper/Expander.cs		\
	wrapper/FontButton.cs		\
	wrapper/FontSelectionDialog.cs	\
	wrapper/Frame.cs		\
	wrapper/HScale.cs		\
	wrapper/HScrollbar.cs		\
	wrapper/Image.cs		\
	wrapper/ImageMenuItem.cs	\
	wrapper/Label.cs		\
	wrapper/MessageDialog.cs	\
	wrapper/MenuItem.cs		\
	wrapper/Notebook.cs		\
	wrapper/Object.cs		\
	wrapper/OptionMenu.cs		\
	wrapper/Paned.cs		\
	wrapper/RadioButton.cs		\
	wrapper/RadioMenuItem.cs	\
	wrapper/RadioToolButton.cs	\
	wrapper/Range.cs		\
	wrapper/Scale.cs		\
	wrapper/ScrolledWindow.cs	\
	wrapper/SpinButton.cs		\
	wrapper/Table.cs		\
	wrapper/TextView.cs		\
	wrapper/ToggleToolButton.cs	\
	wrapper/Toolbar.cs		\
	wrapper/ToolButton.cs		\
	wrapper/Viewport.cs		\
	wrapper/VScale.cs		\
	wrapper/VScrollbar.cs		\
	wrapper/Widget.cs		\
	wrapper/Window.cs

libstetic_dll_resources =		\
	wrapper/pixmaps/*.png		\
	wrapper/objects.xml

libstetic.dll: $(libstetic_dll_sources) $(libstetic_dll_resources)
	@rm -f sources.rsp
	@echo $(libstetic_dll_sources:%=$(srcdir)/%) > sources.rsp
	@for resource in $(libstetic_dll_resources); do echo -resource:$$resource,`basename $$resource` >> sources.rsp; done
	$(MCS) -debug -pkg:gnome-sharp-2.0 -target:library -out:$@ @sources.rsp

CLEANFILES = libstetic.dll libstetic.dll.mdb sources.rsp

## Don't remove the 'COPIED' file below without fixing 'dist-hook'.
EXTRA_DIST =				\
	$(libstetic_dll_sources)	\
	libstetic.dll.config		\
	wrapper/pixmaps/COPIED

dist-hook:
	cp -p $(srcdir)/wrapper/pixmaps/*.png $(distdir)/wrapper/pixmaps/

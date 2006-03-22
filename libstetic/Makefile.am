pkglib_DATA = libstetic.dll libstetic.dll.config libstetic.dll.mdb

libstetic_dll_sources =			\
	AssemblyWidgetLibrary.cs 	\
	ClassDescriptor.cs		\
	CommandDescriptor.cs		\
	CustomWidget.cs		\
	DND.cs				\
	ErrorWidget.cs		\
	EnumDescriptor.cs		\
	EmbedWindow.cs		\
	GeneratorContext.cs			\
	GladeException.cs		\
	GladeUtils.cs			\
	HandleWindow.cs			\
	ImageInfo.cs 			\
	ItemDescriptor.cs		\
	ItemGroup.cs			\
	ItemGroupCollection.cs	\
	IProject.cs			\
	IPropertyEditor.cs			\
	IResourceProvider.cs 	\
	ObjectWrapper.cs		\
	ParamSpec.cs			\
	Placeholder.cs			\
	PreviewBox.cs		\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	PropertyEditorCell.cs	\
	RadioGroupManager.cs		\
	Registry.cs			\
	Set.cs				\
	Shadow.cs				\
	SignalDescriptor.cs		\
	TextBox.cs			\
	Tooltips.cs			\
	TranslatableAttribute.cs	\
	TypedClassDescriptor.cs		\
	TypedPropertyDescriptor.cs		\
	TypedSignalDescriptor.cs		\
	WidgetLibrary.cs	\
	WidgetUtils.cs		\
	editor/Accelerator.cs		\
	editor/Boolean.cs		\
	editor/Char.cs			\
	editor/Color.cs			\
	editor/Enumeration.cs		\
	editor/GroupPicker.cs		\
	editor/Flags.cs			\
	editor/FlagsSelectorDialog.cs			\
	editor/FloatRange.cs		\
	editor/Image.cs			\
	editor/ImageFile.cs		\
	editor/ImageSelector.cs		\
	editor/IntRange.cs		\
	editor/OptIntRange.cs		\
	editor/ResponseId.cs		\
	editor/SelectImageDialog.cs	\
	editor/StockItem.cs		\
	editor/String.cs		\
	editor/StringArray.cs		\
	editor/Text.cs			\
	editor/TextEditor.cs			\
	editor/TextEditorDialog.cs			\
	editor/ThemedIcon.cs			\
	editor/ThemedIconList.cs			\
	editor/Translatable.cs		\
	editor/WidgetSelector.cs		\
	Metacity/ButtonFunction.cs \
	Metacity/ButtonLayout.cs \
	Metacity/FrameFlags.cs \
	Metacity/FrameType.cs \
	Metacity/ObjectManager.cs \
	Metacity/Preview.cs \
	Metacity/Theme.cs \
	wrapper/Bin.cs			\
	wrapper/Box.cs			\
	wrapper/Button.cs		\
	wrapper/ButtonBox.cs		\
	wrapper/CheckButton.cs		\
	wrapper/ColorButton.cs		\
	wrapper/ComboBox.cs		\
	wrapper/ComboBoxEntry.cs	\
	wrapper/Container.cs		\
	wrapper/Custom.cs		\
	wrapper/Dialog.cs		\
	wrapper/Entry.cs		\
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
	wrapper/Signal.cs		\
	wrapper/SignalChangedEventHandler.cs	\
	wrapper/SignalCollection.cs	\
	wrapper/SignalEventHandler.cs	\
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
	wrapper/WidgetEventHandler.cs		\
	wrapper/WidgetNameChangedHandler.cs		\
	wrapper/Window.cs

libstetic_dll_resources =		\
	stetic.glade				\
	wrapper/pixmaps/*.png		\
	wrapper/objects.xml

libstetic.dll: $(libstetic_dll_sources) $(libstetic_dll_resources)
	@rm -f sources.rsp
	@echo $(libstetic_dll_sources:%=$(srcdir)/%) > sources.rsp
	@for resource in $(libstetic_dll_resources); do echo -resource:$$resource,`basename $$resource` >> sources.rsp; done
	$(MCS) -debug -unsafe -pkg:gnome-sharp-2.0 -pkg:gconf-sharp-2.0 -pkg:glade-sharp-2.0 -target:library -out:$@ @sources.rsp

CLEANFILES = libstetic.dll libstetic.dll.mdb sources.rsp

## Don't remove the 'COPIED' file below without fixing 'dist-hook'.
EXTRA_DIST =				\
	$(libstetic_dll_sources)	\
	libstetic.dll.config		\
	wrapper/pixmaps/COPIED

dist-hook:
	cp -p $(srcdir)/wrapper/pixmaps/*.png $(distdir)/wrapper/pixmaps/

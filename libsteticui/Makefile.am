pkglib_DATA = libsteticui.dll libsteticui.dll.config libsteticui.dll.mdb

libsteticui_dll_sources =			\
	Metacity/ButtonFunction.cs \
	Metacity/ButtonLayout.cs \
	Metacity/FrameFlags.cs \
	Metacity/FrameType.cs \
	Metacity/ObjectManager.cs \
	Metacity/Preview.cs \
	Metacity/Theme.cs \
	ActionComponent.cs \
	ActionGroupComponent.cs \
	ActionGroupDesigner.cs \
	ActionGroupDesignerBackend.cs \
	ActionGroupEditSession.cs \
	ActionGroupToolbar.cs \
	Application.cs			\
	ApplicationBackend.cs			\
	ApplicationBackendController.cs \
	Clipboard.cs			\
	ContainerUndoRedoManager.cs \
	ContextMenu.cs			\
	Component.cs			\
	ComponentEventHandler.cs			\
	ComponentSignalEventHandler.cs			\
	ComponentType.cs			\
	CodeGenerator.cs			\
	CodeGeneratorInternalClass.cs			\
	CodeGeneratorPartialClass.cs			\
	EmbedWindow.cs		\
	Glade.cs			\
	Grid.cs				\
	GuiDispatchServerSink.cs \
	GuiDispatchServerSinkProvider.cs \
	NoGuiDispatchAttribute.cs \
	Palette.cs			\
	PaletteBackend.cs			\
	PluggableWidget.cs			\
	Project.cs			\
	ProjectBackend.cs			\
	ProjectView.cs			\
	ProjectViewBackend.cs			\
	PropertyEditor.cs		\
	Shadow.cs				\
	SignalsEditor.cs		\
	SignalsEditorBackend.cs		\
	PropertyGrid.cs			\
	PropertyTree.cs			\
	UndoQueue.cs			\
	UserInterface.cs		\
	WidgetActionBar.cs		\
	WidgetComponent.cs			\
	WidgetDesigner.cs		\
	WidgetDesignerBackend.cs		\
	WidgetEditSession.cs		\
	WidgetFactory.cs		\
	WidgetPropertyTree.cs	\
	WidgetPropertyTreeBackend.cs	\
	WidgetTreeCombo.cs

libsteticui_dll_resources =		\
	action.png		\
	missing.png

libsteticui.dll: $(libsteticui_dll_sources) $(libsteticui_dll_resources)
	@rm -f sources.rsp
	@echo $(libsteticui_dll_sources:%=$(srcdir)/%) > sources.rsp
	@for resource in $(libsteticui_dll_resources); do echo -resource:$$resource,`basename $$resource` >> sources.rsp; done
	$(MCS) -debug -unsafe -main:Stetic.ApplicationBackend -r:Mono.Posix -r:System.Runtime.Remoting -pkg:gnome-sharp-2.0 -pkg:gconf-sharp-2.0 -r:../libstetic/libstetic.dll -out:$@ @sources.rsp

CLEANFILES = libsteticui.dll libsteticui.dll.mdb sources.rsp

EXTRA_DIST =				\
	$(libsteticui_dll_sources)	\
	libsteticui.dll.config

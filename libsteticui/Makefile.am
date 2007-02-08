ASSEMBLY_DIR = $(top_builddir)/bin

ASSEMBLY = $(ASSEMBLY_DIR)/libsteticui.dll

ASSEMBLY_FILES = \
	$(ASSEMBLY) \
	$(ASSEMBLY_DIR)/libsteticui.dll.mdb \
	$(CECIL_DLL) \
	$(DATA_FILE_BUILD)
	
pkglib_DATA = $(ASSEMBLY_FILES)

libsteticui_dll_sources =  \
	ActionComponent.cs \
	ActionGroupComponent.cs \
	ActionGroupDesigner.cs \
	ActionGroupDesignerBackend.cs \
	ActionGroupEditSession.cs \
	ActionGroupToolbar.cs \
	Application.cs \
	ApplicationBackend.cs \
	ApplicationBackendController.cs \
	AssemblyResolver.cs \
	AssemblyWidgetLibrary.cs \
	CecilClassDescriptor.cs \
	CecilPropertyDescriptor.cs \
	CecilSignalDescriptor.cs \
	CecilWidgetLibrary.cs \
	Clipboard.cs \
	CodeGenerationResult.cs \
	CodeGenerator.cs \
	CodeGeneratorInternalClass.cs \
	CodeGeneratorPartialClass.cs \
	Component.cs \
	ComponentEventHandler.cs \
	ComponentSignalEventHandler.cs \
	ComponentType.cs \
	ContainerUndoRedoManager.cs \
	ContextMenu.cs \
	EmbedWindow.cs \
	Glade.cs \
	Grid.cs \
	GuiDispatchServerSink.cs \
	GuiDispatchServerSinkProvider.cs \
	Metacity/ButtonFunction.cs \
	Metacity/ButtonLayout.cs \
	Metacity/FrameFlags.cs \
	Metacity/FrameType.cs \
	Metacity/ObjectManager.cs \
	Metacity/Preview.cs \
	Metacity/Theme.cs \
	Palette.cs \
	PaletteBackend.cs \
	PluggableWidget.cs \
	Project.cs \
	ProjectBackend.cs \
	ProjectView.cs \
	ProjectViewBackend.cs \
	PropertyEditor.cs \
	PropertyGrid.cs \
	PropertyTree.cs \
	Shadow.cs \
	SignalsEditor.cs \
	SignalsEditorBackend.cs \
	UndoQueue.cs \
	UserInterface.cs \
	WidgetActionBar.cs \
	WidgetComponent.cs \
	WidgetDesigner.cs \
	WidgetDesignerBackend.cs \
	WidgetEditSession.cs \
	WidgetFactory.cs \
	WidgetPropertyTree.cs \
	WidgetPropertyTreeBackend.cs \
	WidgetTreeCombo.cs

libsteticui_dll_resources =  \
	action.png \
	missing.png
	

libsteticui_dll_references =  \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:./lib/Mono.Cecil.dll \
	-r:$(top_builddir)/bin/libstetic.dll

libsteticui_dll_datafiles = libsteticui.dll.config

all: $(ASSEMBLY) $(DATA_FILE_BUILD) $(CECIL_DLL)
	
RESOURCE_FILES = $(addprefix $(srcdir)/, $(libsteticui_dll_resources))
DATA_FILES = $(addprefix $(srcdir)/, $(libsteticui_dll_datafiles))

$(ASSEMBLY): $(libsteticui_dll_sources) $(RESOURCE_FILES)
	@rm -f sources.rsp
	@echo $(libsteticui_dll_sources:%=$(srcdir)/%) > sources.rsp
	@for resource in $(RESOURCE_FILES); do echo -resource:$$resource,`basename $$resource` >> sources.rsp; done
	$(GMCS) -debug -unsafe -main:Stetic.ApplicationBackend $(libsteticui_dll_references) -out:$@ @sources.rsp

DATA_FILE_BUILD = $(addprefix $(ASSEMBLY_DIR)/, $(libsteticui_dll_datafiles))

$(DATA_FILE_BUILD): $(srcdir)/$(notdir $@)
	mkdir -p $(ASSEMBLY_DIR)
	cp $(srcdir)/$(subst $(ASSEMBLY_DIR),$(srcdir),$@) $(ASSEMBLY_DIR)/.

CECIL = Mono.Cecil.dll
CECIL_DLL = $(ASSEMBLY_DIR)/$(CECIL)

$(CECIL_DLL): $(srcdir)/lib/$(CECIL)
	mkdir -p $(ASSEMBLY_DIR)
	cp $(srcdir)/lib/$(CECIL) $(ASSEMBLY_DIR)/.

CLEANFILES = $(ASSEMBLY_FILES) sources.rsp

EXTRA_DIST = \
	$(libsteticui_dll_sources)	\
	$(RESOURCE_FILES) \
	$(DATA_FILES) \
	$(srcdir)/lib/$(CECIL)

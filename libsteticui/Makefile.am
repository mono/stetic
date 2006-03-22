pkglib_DATA = libsteticui.dll libsteticui.dll.config libsteticui.dll.mdb

libsteticui_dll_sources =			\
	Clipboard.cs			\
	CodeGenerator.cs			\
	ContextMenu.cs			\
	Glade.cs			\
	Grid.cs				\
	Palette.cs			\
	Project.cs			\
	ProjectView.cs			\
	PropertyEditor.cs		\
	SignalsEditor.cs		\
	PropertyGrid.cs			\
	PropertyTree.cs			\
	WidgetActionBar.cs		\
	WidgetFactory.cs		\
	WidgetPropertyTree.cs	\
	WidgetTreeCombo.cs

libsteticui_dll_resources =		\
	missing.png

libsteticui.dll: $(libsteticui_dll_sources) $(libsteticui_dll_resources)
	@rm -f sources.rsp
	@echo $(libsteticui_dll_sources:%=$(srcdir)/%) > sources.rsp
	@for resource in $(libsteticui_dll_resources); do echo -resource:$$resource,`basename $$resource` >> sources.rsp; done
	$(MCS) -debug -pkg:gnome-sharp-2.0 -r:../libstetic/libstetic.dll -target:library -out:$@ @sources.rsp

CLEANFILES = libsteticui.dll libsteticui.dll.mdb sources.rsp

EXTRA_DIST =				\
	$(libsteticui_dll_sources)	\
	libsteticui.dll.config

noinst_PROGRAMS = stetic.exe
noinst_SCRIPTS = stetic

stetic_exe_SOURCES =			\
	Palette.cs			\
	ParamFlags.cs			\
	ParamSpec.cs			\
	PropertyEditors.cs		\
	PropertyGrid.cs			\
	Stetic.cs			\
	WidgetBox.cs			\
	WidgetFactory.cs		\
	WidgetSite.cs			\
	wrappers/DefaultWidgets.cs	\
	wrappers/Frame.cs		\
	wrappers/HBox.cs		\
	wrappers/HButtonBox.cs		\
	wrappers/HPaned.cs		\
	wrappers/Table.cs		\
	wrappers/VBox.cs		\
	wrappers/VButtonBox.cs		\
	wrappers/VPaned.cs		\
	wrappers/Window.cs

comma = ,
STETIC_RESOURCE_FILES = $(wildcard $(top_srcdir)/pixmaps/*.png)
STETIC_RESOURCES = $(foreach file,$(STETIC_RESOURCE_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_SOURCES) $(STETIC_RESOURCE_FILES)
	$(MCS) -g -pkg:gtk-sharp-2.0 -o stetic.exe $(stetic_exe_SOURCES) $(STETIC_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" $< > $@
	chmod a+x $@

CLEANFILES = stetic stetic.exe.mdb

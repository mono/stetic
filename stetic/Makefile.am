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
	wrapper/DefaultWidgets.cs	\
	wrapper/Frame.cs		\
	wrapper/HBox.cs			\
	wrapper/HButtonBox.cs		\
	wrapper/HPaned.cs		\
	wrapper/Table.cs		\
	wrapper/VBox.cs			\
	wrapper/VButtonBox.cs		\
	wrapper/VPaned.cs		\
	wrapper/Window.cs

comma = ,
STETIC_RESOURCE_FILES = $(wildcard $(top_srcdir)/pixmaps/*.png)
STETIC_RESOURCES = $(foreach file,$(STETIC_RESOURCE_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_SOURCES) $(STETIC_RESOURCE_FILES)
	$(MCS) -g -pkg:gtk-sharp-2.0 -o stetic.exe $(stetic_exe_SOURCES) $(STETIC_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" $< > $@
	chmod a+x $@

CLEANFILES = stetic stetic.exe.mdb

noinst_PROGRAMS = stetic.exe
noinst_SCRIPTS = stetic

stetic_exe_SOURCES =			\
	ContextMenu.cs			\
	Glade.cs			\
	Grid.cs				\
	Palette.cs			\
	Project.cs			\
	ProjectView.cs			\
	PropertyEditor.cs		\
	PropertyGrid.cs			\
	Stetic.cs			\
	WidgetFactory.cs

comma = ,
ICON_FILES = $(wildcard $(srcdir)/*.png)
ICON_RESOURCES = $(foreach file,$(ICON_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_SOURCES) $(ICON_FILES) ../libstetic/libstetic.dll
	$(MCS) -g -pkg:gnome-sharp-2.0 -r:../libstetic/libstetic.dll -o stetic.exe $(stetic_exe_SOURCES) $(ICON_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" $< > $@
	chmod a+x $@

CLEANFILES = stetic stetic.exe.mdb

pkglib_DATA = stetic.exe
bin_SCRIPTS = stetic

stetic_exe_sources =			\
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

stetic.exe: $(stetic_exe_sources) $(ICON_FILES) ../libstetic/libstetic.dll
	$(MCS) -debug -pkg:gnome-sharp-2.0 -r:../libstetic/libstetic.dll -out:stetic.exe $(stetic_exe_sources) $(ICON_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" -e "s:\@pkglibdir\@:$(pkglibdir):" $< > $@
	chmod a+x $@

EXTRA_DIST = $(stetic_exe_sources) *.png stetic.in

CLEANFILES = stetic stetic.exe stetic.exe.mdb

run:
	LD_LIBRARY_PATH=../glue/.libs:$$LD_LIBRARY_PATH MONO_PATH=../libstetic $(MONO) --debug stetic.exe

trace:
	LD_LIBRARY_PATH=../glue/.libs:$$LD_LIBRARY_PATH MONO_PATH=../libstetic $(MONO) --trace --debug stetic.exe

mdb:
	LD_LIBRARY_PATH=../glue/.libs:/opt/debug/lib:$$LD_LIBRARY_PATH MONO_PATH=../libstetic mdb stetic.exe

gdb:
	LD_LIBRARY_PATH=../glue/.libs:/opt/debug/lib:$$LD_LIBRARY_PATH MONO_PATH=../libstetic gdb --args $(MONO) stetic.exe

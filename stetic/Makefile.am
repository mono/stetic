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
RESOURCE_FILES = $(wildcard $(srcdir)/*.png) $(srcdir)/stetic.glade
RESOURCES = $(foreach file,$(RESOURCE_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_sources) $(RESOURCE_FILES) ../libstetic/libstetic.dll
	$(MCS) -debug -pkg:gnome-sharp-2.0 -pkg:glade-sharp-2.0 -r:../libstetic/libstetic.dll -out:stetic.exe $(stetic_exe_sources:%=$(srcdir)/%) $(RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" -e "s:\@pkglibdir\@:$(pkglibdir):" $< > $@
	chmod a+x $@

EXTRA_DIST = $(stetic_exe_sources) stetic.in stetic.glade

dist-hook:
	cp -p $(srcdir)/*.png $(distdir)/

CLEANFILES = stetic stetic.exe stetic.exe.mdb

run:
	LD_LIBRARY_PATH=../glue/.libs:$$LD_LIBRARY_PATH MONO_PATH=../libstetic $(MONO) --debug stetic.exe

trace:
	LD_LIBRARY_PATH=../glue/.libs:$$LD_LIBRARY_PATH MONO_PATH=../libstetic $(MONO) --trace --debug stetic.exe

mdb:
	LD_LIBRARY_PATH=../glue/.libs:/opt/debug/lib:$$LD_LIBRARY_PATH MONO_PATH=../libstetic mdb stetic.exe

gdb:
	LD_LIBRARY_PATH=../glue/.libs:/opt/debug/lib:$$LD_LIBRARY_PATH MONO_PATH=../libstetic gdb --args $(MONO) stetic.exe

noinst_PROGRAMS = stetic.exe
noinst_SCRIPTS = stetic

stetic_exe_SOURCES =		\
	FrameWrapper.cs		\
	HBoxWrapper.cs		\
	HPanedWrapper.cs	\
	Palette.cs		\
	ParamFlags.cs		\
	ParamSpec.cs		\
	PropertyEditors.cs	\
	PropertyGrid.cs		\
	Stetic.cs		\
	TableWrapper.cs		\
	VBoxWrapper.cs		\
	VPanedWrapper.cs	\
	WidgetBox.cs		\
	WidgetFactory.cs	\
	WidgetSite.cs		\
	WindowWrapper.cs

comma = ,
STETIC_RESOURCE_FILES = $(wildcard $(top_srcdir)/pixmaps/*.png)
STETIC_RESOURCES = $(foreach file,$(STETIC_RESOURCE_FILES),-resource:$(file)$(comma)$(notdir $(file)))

stetic.exe: $(stetic_exe_SOURCES) $(STETIC_RESOURCE_FILES)
	$(MCS) -g $(STETIC_LIBS) -o stetic.exe $(stetic_exe_SOURCES) $(STETIC_RESOURCES)

stetic: stetic.in
	sed -e "s:\@MONO\@:$(MONO):" $< > $@
	chmod a+x $@

CLEANFILES = stetic stetic.exe.mdb

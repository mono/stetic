SUBDIRS = . glue

pkg = metacity
glue_includes = metacity-private/preview-widget.h
GAPI_SOURCE = metacity-sources.xml
RAW_API = metacity-api.raw
API = metacity-api.xml
METADATA = Metacity.metadata

ASSEMBLY_NAME = metacity-sharp
ASSEMBLY = $(ASSEMBLY_NAME).dll

build_sources =			\
	Theme.cs

customs = 

CLEANFILES = metacity-sharp.dll test-preview.exe generated-stamp generated/*.cs $(API)

DISTCLEANFILES= AssemblyInfo.cs

pkglib_DATA = metacity-sharp.dll

$(RAW_API): $(GAPI_SOURCE)
	$(GAPI_PARSER) $(GAPI_SOURCE)

$(API): $(RAW_API) $(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(GAPI_FIXUP) --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API)
	 $(GAPI_CODEGEN) --generate $(API) $(GTK_SHARP_CFLAGS) --outdir=generated	\
	 --customdir=$(srcdir) --assembly-name=metacity-sharp				\
	 --gluelib-name=$(pkg)sharpglue-2 --glue-filename=glue/generated.c		\
	 --glue-includes=$(glue_includes) && touch generated-stamp

metacity-sharp.dll: $(build_sources) generated-stamp
	$(MCS) -nowarn:169 -unsafe -target:library $(GTK_SHARP_LIBS) \
		$(build_sources) generated/*.cs -out:metacity-sharp.dll

EXTRA_DIST =			\
	$(GAPI_SOURCE)		\
	$(RAW_API)		\
	$(METADATA)		\
	$(build_sources)	\
	$(customs)

test-preview.exe: test-preview.cs metacity-sharp.dll
	$(MCS) -out:test-preview.exe test-preview.cs -pkg:gtk-sharp-2.0 -r:metacity-sharp.dll

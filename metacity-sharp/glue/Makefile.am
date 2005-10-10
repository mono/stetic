lib_LTLIBRARIES = libmetacitysharpglue-2.la

nodist_libmetacitysharpglue_2_la_SOURCES = generated.c

# Adding a new glue file?
# Please remember to update makefile.win32

libmetacitysharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libmetacitysharpglue_2_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libmetacitysharpglue.dll: $(libmetacitysharpglue_2_la_OBJECTS) libmetacitysharpglue.rc libmetacitysharpglue.def
	./build-dll libmetacitysharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

#EXTRA_DIST = makefile.win32 win32dll.c

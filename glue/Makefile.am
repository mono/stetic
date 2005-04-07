INCLUDES = $(GTK_CFLAGS) -std=c99

lib_LTLIBRARIES = libsteticglue.la

libsteticglue_la_LDFLAGS = -module -avoid-version -no-undefined
libsteticglue_la_LIBADD = $(GTK_LIBS)

libsteticglue_la_SOURCES = 	\
	custom.c		\
	paramspec.c		\
	value.c

/* Copyright (c) 2004 Novell, Inc. */

#include <glib-object.h>

gulong stetic_notify_connect (GObject *object, gpointer callback, gpointer data);
void stetic_notify_disconnect (GObject *object, gulong id);

gulong
stetic_notify_connect (GObject *object, gpointer callback, gpointer data)
{
	return g_signal_connect (object, "notify", G_CALLBACK (callback), data);
}

void
stetic_notify_disconnect (GObject *object, gulong id)
{
	g_signal_handler_disconnect (object, id);
}


/* Copyright (c) 2005 Novell, Inc. */

#include <stdlib.h>
#include <string.h>
#include <glib-object.h>
#include <gtk/gtkcontainer.h>

gboolean stetic_g_value_init_for_property (GValue *value, GType gtype, const char *property_name);
gboolean stetic_g_value_init_for_child_property (GValue *value, GType gtype, const char *property_name);
gboolean stetic_g_value_hydrate (GValue *value, const char *data);

gboolean
stetic_g_value_init_for_property (GValue *value, GType gtype, const char *property_name)
{
	GObjectClass *klass = g_type_class_ref (gtype);
	GParamSpec *spec = g_object_class_find_property (klass, property_name);
	g_type_class_unref (klass);
	if (!spec)
		return FALSE;

	g_value_init (value, spec->value_type);
	return TRUE;
}

gboolean
stetic_g_value_init_for_child_property (GValue *value, GType gtype, const char *property_name)
{
	GtkContainerClass *klass = g_type_class_ref (gtype);
	GParamSpec *spec = gtk_container_class_find_child_property (G_OBJECT_CLASS(klass), property_name);
	g_type_class_unref (klass);
	if (!spec)
		return FALSE;

	g_value_init (value, spec->value_type);
	return TRUE;
}

gboolean
stetic_g_value_hydrate (GValue *value, const char *data)
{
	switch (G_VALUE_TYPE (value)) {
	case G_TYPE_CHAR:
		g_value_set_char (value, strtol (data, NULL, 10));
		break;
	case G_TYPE_UCHAR:
		g_value_set_uchar (value, strtol (data, NULL, 10));
		break;
	case G_TYPE_BOOLEAN:
		g_value_set_boolean (value, !strcmp (data, "True"));
		break;
	case G_TYPE_INT:
		g_value_set_int (value, strtol (data, NULL, 10));
		break;
	case G_TYPE_UINT:
		g_value_set_uint (value, strtoul (data, NULL, 10));
		break;
	case G_TYPE_LONG:
		g_value_set_long (value, strtol (data, NULL, 10));
		break;
	case G_TYPE_ULONG:
		g_value_set_ulong (value, strtoul (data, NULL, 10));
		break;
	case G_TYPE_INT64:
		g_value_set_int64 (value, strtoll (data, NULL, 10));
		break;
	case G_TYPE_UINT64:
		g_value_set_uint64 (value, strtoull (data, NULL, 10));
		break;
	case G_TYPE_FLOAT:
		g_value_set_float (value, strtof (data, NULL));
		break;
	case G_TYPE_DOUBLE:
		g_value_set_double (value, strtod (data, NULL));
		break;
	case G_TYPE_STRING:
		g_value_set_string (value, data);
		break;
	default:
		if (G_TYPE_IS_ENUM (G_VALUE_TYPE (value))) {
			GEnumClass *enum_class = g_type_class_ref (G_VALUE_TYPE(value));
			GEnumValue *enum_value = g_enum_get_value_by_name (enum_class, data);
			g_type_class_unref (enum_class);
			if (enum_value) {
				g_value_set_enum (value, enum_value->value);
				return TRUE;
			}
		}
		return FALSE;
	}

	return TRUE;
}

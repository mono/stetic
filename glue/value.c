/* Copyright (c) 2005 Novell, Inc. */

#include <stdlib.h>
#include <string.h>
#include <glib-object.h>
#include <gtk/gtkcontainer.h>

gboolean stetic_g_value_parse_property (GValue *value, GObjectClass *klass, const char *property_name, const char *data);
gboolean stetic_g_value_parse_child_property (GValue *value, GObjectClass *klass, const char *property_name, const char *data);
gboolean stetic_g_value_parse (GValue *value, const char *data);

char *stetic_g_value_child_property_to_string (GtkContainer *parent, GtkWidget *child, const char *property_name);
char *stetic_g_value_property_to_string (GtkWidget *widget, const char *property_name);
char *stetic_g_value_to_string (GValue *value);

gboolean
stetic_g_value_parse_property (GValue *value, GObjectClass *klass, const char *property_name, const char *data)
{
	GParamSpec *spec = g_object_class_find_property (klass, property_name);
	if (!spec)
		return FALSE;

	g_value_init (value, spec->value_type);
	return stetic_g_value_parse (value, data);
}

gboolean
stetic_g_value_parse_child_property (GValue *value, GObjectClass *klass, const char *property_name, const char *data)
{
	GParamSpec *spec = gtk_container_class_find_child_property (G_OBJECT_CLASS (klass), property_name);
	if (!spec)
		return FALSE;

	g_value_init (value, spec->value_type);
	return stetic_g_value_parse (value, data);
}

gboolean
stetic_g_value_parse (GValue *value, const char *data)
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
			GEnumClass *enum_class = g_type_class_ref (G_VALUE_TYPE (value));
			GEnumValue *enum_value = g_enum_get_value_by_name (enum_class, data);
			g_type_class_unref (enum_class);
			if (enum_value) {
				g_value_set_enum (value, enum_value->value);
				return TRUE;
			}
		} else if (G_TYPE_IS_FLAGS (G_VALUE_TYPE (value))) {
			GFlagsClass *flags_class = g_type_class_ref (G_VALUE_TYPE (value));
			char **flags;
			guint i, fval = 0;

			flags = g_strsplit (data, "|", 0);
			for (i = 0; flags[i]; i++) {
				GFlagsValue *flags_value = g_flags_get_value_by_nick (flags_class, flags[i]);
				if (flags_value)
					fval |= flags_value->value;
			}
			g_strfreev (flags);

			g_value_set_flags (value, fval);
			g_type_class_unref (flags_class);
			return TRUE;
		}

		return FALSE;
	}

	return TRUE;
}

char *
stetic_g_value_child_property_to_string (GtkContainer *container, GtkWidget *child,
					 const char *property_name)
{
	GParamSpec *pspec = gtk_container_class_find_child_property (G_OBJECT_GET_CLASS (container), property_name);
	GValue value;
	char *strval;

	memset (&value, 0, sizeof (GValue));
	g_value_init (&value, pspec->value_type);
	gtk_container_child_get_property (container, child, property_name, &value);
	strval = stetic_g_value_to_string (&value);
	g_value_unset (&value);

	return strval;
}

char *
stetic_g_value_property_to_string (GtkWidget *widget, const char *property_name)
{
	GParamSpec *pspec = g_object_class_find_property (G_OBJECT_GET_CLASS (widget), property_name);
	GValue value;
	char *strval;

	memset (&value, 0, sizeof (GValue));
	g_value_init (&value, pspec->value_type);
	g_object_get_property (widget, property_name, &value);
	strval = stetic_g_value_to_string (&value);
	g_value_unset (&value);

	return strval;
}

char *
stetic_g_value_to_string (GValue *value)
{
	switch (G_VALUE_TYPE (value)) {
	case G_TYPE_CHAR:
		return g_strdup_printf ("%d", g_value_get_char (value));
	case G_TYPE_UCHAR:
		return g_strdup_printf ("%u", g_value_get_uchar (value));
	case G_TYPE_BOOLEAN:
		return g_strdup (g_value_get_boolean (value) ? "True" : "False");
	case G_TYPE_INT:
		return g_strdup_printf ("%d", g_value_get_int (value));
	case G_TYPE_UINT:
		return g_strdup_printf ("%u", g_value_get_uint (value));
	case G_TYPE_LONG:
		return g_strdup_printf ("%ld", g_value_get_long (value));
	case G_TYPE_ULONG:
		return g_strdup_printf ("%lu", g_value_get_ulong (value));
	case G_TYPE_INT64:
		return g_strdup_printf ("%lld", g_value_get_int64 (value));
	case G_TYPE_UINT64:
		return g_strdup_printf ("%llu", g_value_get_uint64 (value));
	case G_TYPE_FLOAT:
		return g_strdup_printf ("%g", g_value_get_float (value));
	case G_TYPE_DOUBLE:
		return g_strdup_printf ("%g", g_value_get_double (value));
	case G_TYPE_STRING:
		return g_value_dup_string (value);
	default:
		if (G_TYPE_IS_ENUM (G_VALUE_TYPE (value))) {
			GEnumClass *enum_class = g_type_class_ref (G_VALUE_TYPE (value));
			GEnumValue *enum_value = g_enum_get_value (enum_class, g_value_get_enum (value));
			g_type_class_unref (enum_class);
			if (enum_value)
				return g_strdup (enum_value->value_name);
		} else if (G_TYPE_IS_FLAGS (G_VALUE_TYPE (value))) {
			GFlagsClass *flags_class = g_type_class_ref (G_VALUE_TYPE (value));
			GString *flags = g_string_new (NULL);
			guint i, val = g_value_get_flags (value);

			for (i = 0; i < flags_class->n_values; i++) {
				if ((val & flags_class->values[i].value) == flags_class->values[i].value) {
					if (flags->len != 0)
						g_string_append_c (flags, '|');
					g_string_append (flags, flags_class->values[i].value_nick);
				}
			}
			return g_string_free (flags, FALSE);
		}
		break;
	}

	return NULL;
}

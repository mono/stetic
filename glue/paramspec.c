/* Copyright (c) 2004 Novell, Inc. */

#include <glib-object.h>
#include <gtk/gtkcontainer.h>

GParamSpec *stetic_param_spec_for_property (GObject *obj, const char *property_name);
GParamSpec *stetic_param_spec_for_child_property (GObject *obj, const char *property_name);

GParamSpec *
stetic_param_spec_for_property (GObject *obj, const char *property_name)
{
	
	return g_object_class_find_property (G_OBJECT_GET_CLASS (obj), property_name);
}

GParamSpec *
stetic_param_spec_for_child_property (GObject *obj, const char *property_name)
{
	return gtk_container_class_find_child_property (G_OBJECT_GET_CLASS (obj), property_name);
}

GParamFlags stetic_param_spec_get_flags (GParamSpec *pspec);
GType stetic_param_spec_get_value_type (GParamSpec *pspec);
GType stetic_param_spec_get_owner_type (GParamSpec *pspec);

GParamFlags
stetic_param_spec_get_flags (GParamSpec *pspec)
{
	return pspec->flags;
}

GType
stetic_param_spec_get_value_type (GParamSpec *pspec)
{
	return pspec->value_type;
}

GType
stetic_param_spec_get_owner_type (GParamSpec *pspec)
{
	return pspec->owner_type;
}

#define STETIC_PARAM_SPEC_RANGE_TYPE(name, typename, type)			\
type stetic_param_spec_##name##_get_minimum (GParamSpec##typename *pspec);	\
type stetic_param_spec_##name##_get_maximum (GParamSpec##typename *pspec);	\
type stetic_param_spec_##name##_get_default (GParamSpec##typename *pspec);	\
										\
type										\
stetic_param_spec_##name##_get_minimum (GParamSpec##typename *pspec)		\
{										\
	return pspec->minimum;							\
}										\
										\
type										\
stetic_param_spec_##name##_get_maximum (GParamSpec##typename *pspec)		\
{										\
	return pspec->maximum;							\
}										\
										\
type										\
stetic_param_spec_##name##_get_default (GParamSpec##typename *pspec)		\
{										\
	return pspec->default_value;						\
}

#define STETIC_PARAM_SPEC_DEFAULTABLE_TYPE(name, typename, type)		\
type stetic_param_spec_##name##_get_default (GParamSpec##typename *pspec);	\
										\
type										\
stetic_param_spec_##name##_get_default (GParamSpec##typename *pspec)		\
{										\
	return pspec->default_value;						\
}

#define STETIC_PARAM_SPEC_ACCESSOR(name, typename, type, field)		\
type stetic_param_spec_##name##_get_##field (GParamSpec##typename *pspec);	\
										\
type										\
stetic_param_spec_##name##_get_##field (GParamSpec##typename *pspec)		\
{										\
	return pspec->field;							\
}

STETIC_PARAM_SPEC_RANGE_TYPE (char, Char, char)
STETIC_PARAM_SPEC_RANGE_TYPE (uchar, UChar, guchar)
STETIC_PARAM_SPEC_DEFAULTABLE_TYPE (boolean, Boolean, gboolean)
STETIC_PARAM_SPEC_RANGE_TYPE (int, Int, int)
STETIC_PARAM_SPEC_RANGE_TYPE (uint, UInt, guint)
STETIC_PARAM_SPEC_RANGE_TYPE (long, Long, long)
STETIC_PARAM_SPEC_RANGE_TYPE (ulong, ULong, gulong)
STETIC_PARAM_SPEC_RANGE_TYPE (int64, Int64, gint64)
STETIC_PARAM_SPEC_RANGE_TYPE (uint64, UInt64, guint64)
STETIC_PARAM_SPEC_DEFAULTABLE_TYPE (unichar, Unichar, gunichar)
STETIC_PARAM_SPEC_DEFAULTABLE_TYPE (enum, Enum, int)
STETIC_PARAM_SPEC_DEFAULTABLE_TYPE (flags, Flags, guint)
STETIC_PARAM_SPEC_RANGE_TYPE (float, Float, float)
STETIC_PARAM_SPEC_ACCESSOR (float, Float, float, epsilon)
STETIC_PARAM_SPEC_RANGE_TYPE (double, Double, double)
STETIC_PARAM_SPEC_ACCESSOR (double, Double, double, epsilon)
STETIC_PARAM_SPEC_DEFAULTABLE_TYPE (string, String, const char *)
STETIC_PARAM_SPEC_ACCESSOR (value_array, ValueArray, guint, fixed_n_elements)
STETIC_PARAM_SPEC_ACCESSOR (value_array, ValueArray, GParamSpec *, element_spec)
STETIC_PARAM_SPEC_ACCESSOR (override, Override, GParamSpec *, overridden)

int stetic_param_spec_enum_get_minimum (GParamSpecEnum *pspec);
int stetic_param_spec_enum_get_maximum (GParamSpecEnum *pspec);

int
stetic_param_spec_enum_get_minimum (GParamSpecEnum *pspec)
{
	return pspec->enum_class->minimum;
}

int
stetic_param_spec_enum_get_maximum (GParamSpecEnum *pspec)
{
	return pspec->enum_class->maximum;
}

const char *stetic_param_spec_enum_get_value_name (GParamSpecEnum *pspec, int value);

const char *
stetic_param_spec_enum_get_value_name (GParamSpecEnum *pspec, int value)
{
	GEnumValue *ev;

	ev = g_enum_get_value (pspec->enum_class, value);
	if (ev)
		return ev->value_nick;
	else
		return NULL;
}

guint stetic_param_spec_flags_get_mask (GParamSpecFlags *pspec);

guint
stetic_param_spec_flags_get_mask (GParamSpecFlags *pspec)
{
	return pspec->flags_class->mask;
}

const char *stetic_param_spec_flags_get_value_name (GParamSpecFlags *pspec, guint value);

const char *
stetic_param_spec_flags_get_value_name (GParamSpecFlags *pspec, guint value)
{
	GFlagsValue *fv;

	fv = g_flags_get_first_value (pspec->flags_class, value);
	if (fv)
		return fv->value_nick;
	else
		return NULL;
}

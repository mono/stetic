#include <gtk/gtkdrawingarea.h>

/* This is done in C, because it's tiny, and it's much simpler to make
 * glade import/export work if the registered name of the class is just
 * "Custom", and GLib.Object won't let us do that.
 */

typedef struct {
	GtkDrawingArea parent;
} Custom;

typedef struct {
	GtkDrawingAreaClass parent_class;
} CustomClass;

GType custom_get_type (void);
Custom *custom_new (void);
static void custom_realize (GtkWidget *widget);
static void custom_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec);

G_DEFINE_TYPE (Custom, custom, GTK_TYPE_DRAWING_AREA)

static void
custom_class_init (CustomClass *klass)
{
	GtkWidgetClass *widget_class = GTK_WIDGET_CLASS (klass);
	GObjectClass *object_class = G_OBJECT_CLASS (klass);

	object_class->set_property = custom_set_property;
	widget_class->realize = custom_realize;

	g_object_class_install_property (object_class, 1,
					 g_param_spec_string ("last_modification_time",
							      NULL, NULL, NULL,
							      G_PARAM_WRITABLE));
}

static void
custom_init (Custom *custom)
{
	;
}

Custom *
custom_new (void)
{
	return g_object_new (custom_get_type (), NULL);
}

static void
custom_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
}

/* from glade */
static char * custom_bg_xpm[] = {
"8 8 4 1",
" 	c None",
".	c #BBBBBB",
"+	c #D6D6D6",
"@	c #6B5EFF",
".+..+...",
"+..@@@..",
"..@...++",
"..@...++",
"+.@..+..",
".++@@@..",
"..++....",
"..++...."};

static void
custom_realize (GtkWidget *widget)
{
	GTK_WIDGET_CLASS (custom_parent_class)->realize (widget);

	GdkPixmap *pixmap;
	pixmap = gdk_pixmap_create_from_xpm_d (widget->window, NULL, NULL, custom_bg_xpm);
	gdk_window_set_back_pixmap (widget->window, pixmap, FALSE);
}

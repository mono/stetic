noinst_DATA = libstetic.dll

libstetic_dll_sources =			\
	IContainerWrapper.cs		\
	IContextMenuProvider.cs		\
	IObjectWrapper.cs		\
	IStetic.cs			\
	IWidgetSite.cs			\
	IWidgetWrapper.cs		\
	IWindowWrapper.cs		\
	ParamFlags.cs			\
	ParamSpec.cs			\
	PropertyDescriptor.cs		\
	PropertyEditorAttribute.cs	\
	PropertyGroup.cs		\
	WidgetBox.cs			\
	WidgetSite.cs			\
	WidgetWrapperAttribute.cs	\
	editor/Boolean.cs		\
	editor/Char.cs			\
	editor/Enumeration.cs		\
	editor/File.cs			\
	editor/FloatRange.cs		\
	editor/IntRange.cs		\
	editor/OptIntRange.cs		\
	editor/StockItem.cs		\
	editor/String.cs

libstetic.dll: $(libstetic_dll_sources)
	$(MCS) -g -pkg:gnome-sharp-2.0 -target:library -o $@ $(libstetic_dll_sources)

CLEANFILES = libstetic.dll libstetic.dll.mdb

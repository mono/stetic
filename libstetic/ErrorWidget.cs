using System;
using System.Xml;
using System.CodeDom;
using Mono.Unix;

namespace Stetic
{
	// This widget is shown in place of widgets with unknown classes. 
	
	public class ErrorWidget: Gtk.Frame
	{
		readonly string className;
		readonly Exception exc;
		
		public ErrorWidget (Exception ex)
		{
			exc = ex;
			Init (Catalog.GetString ("Load Error:") + " " + ex.Message);
		}
		
		public ErrorWidget (string className)
		{
			this.className = className;
			Init (Catalog.GetString ("Unknown widget:") + " " + className);
		}
		
		void Init (string message)
		{
			Gtk.Label lab = new Gtk.Label ();
			lab.Markup = "<b><span foreground='red'>" + message + "</span></b>";
			this.CanFocus = false;
			Add (lab);
			this.ShadowType = Gtk.ShadowType.In;
			ShowAll ();
		}
		
		public string ClassName {
			get { return className; }
		}
		
		public Exception Exception {
			get { return exc; }
		}
	}
	
	internal class ErrorWidgetWrapper: Wrapper.Widget
	{
		XmlElement elementData;
		FileFormat format;
		
		public override void Read (ObjectReader reader, XmlElement elem)
		{
			elementData = elem;
			this.format = reader.Format;
		}

		public override XmlElement Write (ObjectWriter writer)
		{
			if (writer.Format != this.format)
				throw new InvalidOperationException ("Can't export incomplete widget information");
				
			return (XmlElement) writer.XmlDocument.ImportNode (elementData, true);
		}
		
		internal protected override void GenerateBuildCode (GeneratorContext ctx, string varName)
		{
			ErrorWidget ew = (ErrorWidget) Wrapped;
			if (ew.Exception != null)
				throw new InvalidOperationException ("Can't generate code for an invalid widget. The widget failed to load: " + ew.Exception.Message);
			else
				throw new InvalidOperationException ("Can't generate code for unknown type: " + ew.ClassName);
		}
	}
}

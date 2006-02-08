using System;
using System.Reflection;
using System.Xml;

namespace Stetic
{
	
	public class SignalDescriptor: ItemDescriptor
	{
		string name, label, description;
		EventInfo eventInfo;
		MethodInfo handler;
		string gladeName;
		
		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		
		public SignalDescriptor (XmlElement elem, ItemGroup group, ClassDescriptor klass) : base (elem, group, klass)
		{
			name = elem.GetAttribute ("name");
			label = elem.GetAttribute ("label");
			description = elem.GetAttribute ("description");

			eventInfo = FindEvent (klass.WrapperType, klass.WrappedType, name);
			handler = eventInfo.EventHandlerType.GetMethod ("Invoke");
			
			if (elem.HasAttribute ("glade-name"))
				gladeName = elem.GetAttribute ("glade-name");
			else {
				object[] att = eventInfo.GetCustomAttributes (typeof(GLib.SignalAttribute), true);
				if (att.Length > 0)
					gladeName = ((GLib.SignalAttribute)att[0]).CName;
			}
		}
		
		public override string Name {
			get { return name; }
		}

		public string Label {
			get { return label; }
		}

		public string Description {
			get { return description; }
		}
		
		public string GladeName {
			get { return gladeName; }
		}

		public Type HandlerType {
			get { return eventInfo.EventHandlerType; }
		}
		
		public Type HandlerReturnType {
			get { return handler.ReturnType; }
		}

		public ParameterInfo[] HandlerParameters {
			get { return handler.GetParameters (); }
		}

		static EventInfo FindEvent (Type wrapperType, Type objectType, string name)
		{
			EventInfo info;

			if (wrapperType != null) {
				info = wrapperType.GetEvent (name, flags);
				if (info != null)
					return info;
			}

			info = objectType.GetEvent (name, flags);
			if (info != null)
				return info;

			throw new ArgumentException ("Invalid event name " + objectType.Name + "." + name);
		}
	}
	
}

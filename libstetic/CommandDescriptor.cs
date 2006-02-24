using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Stetic {

	public class CommandDescriptor : ItemDescriptor {

		string name, checkName, label, description;

		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public CommandDescriptor (XmlElement elem, ItemGroup group, ClassDescriptor klass) : base (elem, group, klass)
		{
			name = elem.GetAttribute ("name");
			label = elem.GetAttribute ("label");
			description = elem.GetAttribute ("description");
			checkName = elem.GetAttribute ("check");
		}

		public override string Name {
			get {
				return name;
			}
		}

		public string Label {
			get {
				return label;
			}
		}

		public string Description {
			get {
				return description;
			}
		}

		public bool Enabled (object obj)
		{
			if (checkName == "")
				return EnabledFor (obj);
			else
				return (bool) InvokeMethod (ObjectWrapper.Lookup (obj), checkName, null, false);
		}

		public bool Enabled (object obj, Gtk.Widget context)
		{
			if (checkName == "")
				return EnabledFor (obj);

			ObjectWrapper wrapper = ObjectWrapper.Lookup (obj);
			return (bool) InvokeMethod (wrapper, checkName, context, true);
		}

		public void Run (object obj)
		{
			InvokeMethod (ObjectWrapper.Lookup (obj), name, null, false);
		}

		public void Run (object obj, Gtk.Widget context)
		{
			ObjectWrapper wrapper = ObjectWrapper.Lookup (obj);
			InvokeMethod (wrapper, name, context, true);
		}
		
		object InvokeMethod (object target, string name, object context, bool withContext)
		{
			if (withContext) {
				MethodInfo metc = target.GetType().GetMethod (name, flags, null, new Type[] {typeof(Gtk.Widget)}, null);
				if (metc != null)
					return metc.Invoke (target, new object[] { context });
			}
			
			MethodInfo met = target.GetType().GetMethod (name, flags, null, Type.EmptyTypes, null);
			if (met != null)
				return met.Invoke (target, new object[0]);
			
			throw new ArgumentException ("Invalid command or checker name. Method '" + name +"' not found in class '" + target.GetType() + "'");
		}
	}
}

using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Stetic {

	public class CommandDescriptor : ItemDescriptor {

		string name, label, description;
		bool needsContext;
		MethodInfo checkInfo, doInfo;

		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public CommandDescriptor (XmlElement elem, ItemGroup group, ClassDescriptor klass) : base (elem, group, klass)
		{
			name = elem.GetAttribute ("name");
			label = elem.GetAttribute ("label");
			description = elem.GetAttribute ("description");

			doInfo = klass.WrapperType.GetMethod (name, flags, null, new Type[] { typeof (Gtk.Widget) }, null);
			if (doInfo != null)
				needsContext = true;
			else
				doInfo = klass.WrapperType.GetMethod (name, flags, null, new Type[0], null);
			if (doInfo == null)
				throw new ArgumentException ("Invalid command name " + klass.WrapperType.Name + "." + name);

			if (elem.GetAttribute ("check") != "") {
				checkInfo = klass.WrapperType.GetMethod (elem.GetAttribute ("check"), flags, null, needsContext ? new Type[] { typeof (Gtk.Widget) } : new Type[0], null);
				if (checkInfo == null)
					throw new ArgumentException ("Invalid checker name " + elem.GetAttribute ("check") + " for command " + klass.WrapperType.Name + "." + name);
			}
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

		public bool NeedsContext {
			get {
				return needsContext;
			}
		}

		public bool Enabled (object obj)
		{
			if (checkInfo == null)
				return EnabledFor (obj);
			else
				return (bool)checkInfo.Invoke (ObjectWrapper.Lookup (obj), new object[0]);
		}

		public bool Enabled (object obj, Gtk.Widget context)
		{
			if (checkInfo == null)
				return EnabledFor (obj);

			ObjectWrapper wrapper = ObjectWrapper.Lookup (obj);
			if (needsContext)
				return (bool)checkInfo.Invoke (wrapper, new object[] { context });
			else
				return (bool)checkInfo.Invoke (wrapper, new object[0]);
		}

		public void Run (object obj)
		{
			doInfo.Invoke (ObjectWrapper.Lookup (obj), new object[0]);
		}

		public void Run (object obj, Gtk.Widget context)
		{
			ObjectWrapper wrapper = ObjectWrapper.Lookup (obj);
			if (needsContext)
				doInfo.Invoke (wrapper, new object[] { context });
			else
				doInfo.Invoke (wrapper, new object[0]);
		}
	}
}

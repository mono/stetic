using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class CommandDescriptor : ItemDescriptor {

		string name, label, description;
		bool needsContext;
		MethodInfo checkInfo, doInfo;

		public CommandDescriptor (Type wrapperType, string commandName)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			name = commandName;

			doInfo = wrapperType.GetMethod (commandName, flags, null, new Type[] { typeof (Stetic.IWidgetSite) }, null);
			if (doInfo != null)
				needsContext = true;
			else
				doInfo = wrapperType.GetMethod (commandName, flags, null, new Type[0], null);
			if (doInfo == null)
				throw new ArgumentException ("Invalid command name " + wrapperType.Name + "." + commandName);

			foreach (CommandAttribute cattr in doInfo.GetCustomAttributes (typeof (CommandAttribute), false)) {
				label = cattr.Name;
				description = cattr.Description;
				if (cattr.Checker != null) {
					checkInfo = wrapperType.GetMethod (cattr.Checker, flags, null, needsContext ? new Type[] { typeof (Stetic.IWidgetSite) } : new Type[0], null);
					if (checkInfo == null)
						throw new ArgumentException ("Invalid checker name " + cattr.Checker + " for command " + wrapperType.Name + "." + commandName);
				}
				break;
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

		public bool Enabled (ObjectWrapper wrapper)
		{
			if (checkInfo == null)
				return EnabledFor (wrapper);
			else
				return (bool)checkInfo.Invoke (wrapper, new object[0]);
		}

		public bool Enabled (ObjectWrapper wrapper, IWidgetSite context)
		{
			if (checkInfo == null)
				return EnabledFor (wrapper);
			else if (needsContext)
				return (bool)checkInfo.Invoke (wrapper, new object[] { context });
			else
				return (bool)checkInfo.Invoke (wrapper, new object[0]);
		}

		public void Run (ObjectWrapper wrapper)
		{
			doInfo.Invoke (wrapper, new object[0]);
		}

		public void Run (ObjectWrapper wrapper, IWidgetSite context)
		{
			if (needsContext)
				doInfo.Invoke (wrapper, new object[] { context });
			else
				doInfo.Invoke (wrapper, new object[0]);
		}
	}
}

using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class CommandDescriptor : ItemDescriptor {

		string name, label;
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
				label = cattr.Label;
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
				return (bool)checkInfo.Invoke (obj, new object[0]);
		}

		public bool Enabled (object obj, IWidgetSite context)
		{
			if (checkInfo == null)
				return EnabledFor (obj);
			else if (needsContext)
				return (bool)checkInfo.Invoke (obj, new object[] { context });
			else
				return (bool)checkInfo.Invoke (obj, new object[0]);
		}

		public void Run (object obj)
		{
			doInfo.Invoke (obj, new object[0]);
		}

		public void Run (object obj, IWidgetSite context)
		{
			if (needsContext)
				doInfo.Invoke (obj, new object[] { context });
			else
				doInfo.Invoke (obj, new object[0]);
		}
	}
}

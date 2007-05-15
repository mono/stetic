using System;
using System.Collections;
using System.Xml;
using System.Reflection;
using Mono.Cecil;

namespace Stetic
{
	class CecilSignalDescriptor: Stetic.SignalDescriptor
	{
		public CecilSignalDescriptor (CecilWidgetLibrary lib, XmlElement elem, Stetic.ItemGroup group, Stetic.ClassDescriptor klass, EventDefinition sinfo) : base (elem, group, klass)
		{
			if (sinfo != null) {
				handlerTypeName = sinfo.EventType.FullName;
				Type t = Registry.GetType (handlerTypeName, false);
				if (t != null) {
					MethodInfo mi = t.GetMethod ("Invoke");
					handlerReturnTypeName = mi.ReturnType.FullName;
					ParameterInfo[] pars = mi.GetParameters ();
					handlerParameters = new ParameterDescriptor [pars.Length];
					for (int n=0; n<pars.Length; n++)
						handlerParameters [n] = new ParameterDescriptor (pars[n].Name, pars[n].ParameterType.FullName);
				} else {
					TypeDefinition td = lib.FindTypeDefinition (handlerTypeName);
					MethodDefinition mi = null;
					foreach (MethodDefinition md in td.Methods) {
						if (md.Name == "Invoke") {
							mi = md;
							break;
						}
					}
					if (mi != null) {
						handlerReturnTypeName = mi.ReturnType.ReturnType.FullName;
						handlerParameters = new ParameterDescriptor [mi.Parameters.Count];
						for (int n=0; n<handlerParameters.Length; n++) {
							ParameterDefinition par = mi.Parameters [n];
							handlerParameters [n] = new ParameterDescriptor (par.Name, par.ParameterType.FullName);
						}
					}
				}
				SaveCecilXml (elem);
			}
			else {
				handlerTypeName = elem.GetAttribute ("handlerTypeName");
				handlerReturnTypeName = elem.GetAttribute ("handlerReturnTypeName");
				
				ArrayList list = new ArrayList ();
				foreach (XmlNode npar in elem.ChildNodes) {
					XmlElement epar = npar as XmlElement;
					if (epar == null) continue;
					list.Add (new ParameterDescriptor (epar.GetAttribute ("name"), epar.GetAttribute ("type")));
				}
				
				handlerParameters = (ParameterDescriptor[]) list.ToArray (typeof(ParameterDescriptor));
			}
			
			Load (elem);
		}

		internal void SaveCecilXml (XmlElement elem)
		{
			elem.SetAttribute ("handlerTypeName", handlerTypeName);
			elem.SetAttribute ("handlerReturnTypeName", handlerReturnTypeName);
			foreach (ParameterDescriptor par in handlerParameters) {
				XmlElement epar = elem.OwnerDocument.CreateElement ("param");
				epar.SetAttribute ("name", par.Name);
				epar.SetAttribute ("type", par.TypeName);
				elem.AppendChild (epar);
			}
		}
	}
}

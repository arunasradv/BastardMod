using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;

namespace BastardsMod.Helpers
{    public static class Common
    {
        public static object UseMethod(object obj, string method_name, params object[] parameters)
        {
            MethodInfo theMethod = obj.GetType().GetMethod(method_name, BindingFlags.NonPublic | BindingFlags.Instance);
            return theMethod.Invoke(obj, parameters);
        }

        public static object UseMethodWithOut(object obj, string method_name, out TextObject parameter, params object[] parameters)
        {
            parameter = new TextObject();
            Type[] vTypes = parameters == null ? new Type[] { parameter.GetType().MakeByRefType() } : new Type[] { parameter.GetType().MakeByRefType(), parameters.GetType() };
            object[] vParms = parameters == null ? new object[1] : new object[] { null, parameters };
            MethodInfo theMethod = obj.GetType().GetMethod(method_name, BindingFlags.NonPublic | BindingFlags.Instance, null, vTypes, null);
            bool retval = (bool)theMethod.Invoke(obj, BindingFlags.InvokeMethod, null, vParms, null);
            parameter = (TextObject)vParms[0];
            return retval;
        }
    }
}

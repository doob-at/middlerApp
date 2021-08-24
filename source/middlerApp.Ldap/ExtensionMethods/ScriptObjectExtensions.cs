using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Scriban.Runtime;

namespace MiddlerApp.Ldap.ExtensionMethods
{
    public static class ScriptObjectExtensions
    {
        public static ScriptObject AddClassInstance<T>(this ScriptObject scriptObject, T instance) where T: class
        {
            var methodInfos = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methodInfos)
            {
                scriptObject.Import(methodInfo.Name, createDelegate(methodInfo, instance));
            }

            return scriptObject;
        }

        private static Delegate createDelegate(MethodInfo methodInfo, object target)
        {

            
            var methodParameters = methodInfo.GetParameters();
            var arguments = new List<Type>(methodParameters.Select(p => p.ParameterType));
            arguments.Add(methodInfo.ReturnType);
            if (methodInfo.ReturnType == typeof(void))
            {
                var action = Expression.GetActionType(arguments.ToArray());
                return Delegate.CreateDelegate(action, target, methodInfo);
            }
            else
            {
                
                var func = Expression.GetFuncType(arguments.ToArray());
                return Delegate.CreateDelegate(func, target, methodInfo);
            }
            
        }
    }
}

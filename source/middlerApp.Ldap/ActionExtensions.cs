using System;
using MiddlerApp.Ldap.Helpers;

namespace MiddlerApp.Ldap
{
    
        public static class ActionExtensions
        {
            public static T InvokeAction<T>(this Action<T> action, T instance = default) => ActionHelpers.InvokeAction(action, instance);
        }
    
}

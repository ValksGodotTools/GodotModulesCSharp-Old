using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotModules
{
    public static class ReflectionUtils
    {
        public static Dictionary<TKey, TValue> LoadInstances<TKey, TValue>(string prefix) =>
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => typeof(TValue).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance).Cast<TValue>()
                .ToDictionary(x => (TKey)Enum.Parse(typeof(TKey), x.GetType().Name.Replace(prefix, "")), x => x);
    }
}
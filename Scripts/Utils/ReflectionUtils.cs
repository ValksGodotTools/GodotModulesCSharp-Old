using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotModules 
{
    public static class ReflectionUtils 
    {
        public static Dictionary<Key, Value> LoadInstances<Key, Value, Namespace>() =>
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => typeof(Value).IsAssignableFrom(x) && !x.IsAbstract && x.Namespace == typeof(Namespace).Namespace)
                .Select(Activator.CreateInstance).Cast<Value>()
                .ToDictionary(x => (Key)Enum.Parse(typeof(Key), x.GetType().Name.Replace(typeof(Value).Name, "")), x => x);
    }
}
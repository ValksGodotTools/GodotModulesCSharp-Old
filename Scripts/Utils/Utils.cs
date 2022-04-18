using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

namespace GodotModules
{
    public static class Utils
    {
        /// <summary>
        /// Put a Dict<Key, Value> into a nice string format for printing to console.
        /// </summary>
        /// <param name="dict"></param>
        /// <typeparam name="Key"></typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <returns></returns>
        public static string StringifyDict<Key, Value>(Dictionary<Key, Value> dict) => string.Join(" ", dict.Select(x => $"{x.Key} {x.Value}"));

        /// <summary>
        /// Remap a value from one range to another
        /// </summary>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns></returns>
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Lerp between two Vector2's
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            float retX = Mathf.Lerp(a.x, b.x, t);
            float retY = Mathf.Lerp(a.y, b.y, t);
            return new Vector2(retX, retY);
        }

        /// <summary>
        /// Creates instances of a type and puts them into a Dict<Key, Instance>
        /// </summary>
        /// <typeparam name="Key"></typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <typeparam name="Namespace">Only add types from this namespace</typeparam>
        /// <returns></returns>
        public static Dictionary<Key, Value> LoadInstances<Key, Value, Namespace>() =>
            typeof(Value).Assembly.GetTypes()
                .Where(x => typeof(Value).IsAssignableFrom(x) && !x.IsAbstract && x.Namespace == typeof(Namespace).Namespace)
                .Select(Activator.CreateInstance).Cast<Value>()
                .ToDictionary(x => (Key)Enum.Parse(typeof(Key), x.GetType().Name.Replace(typeof(Value).Name, "")), x => x);
    }
}
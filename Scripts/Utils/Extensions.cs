using System.Collections.Generic;
using System.Linq;

namespace GodotModules 
{
    public static class Extensions 
    {
        public static string Print<T>(this List<T> list) => string.Join(", ", list);
        public static string Print<Key, Value>(this Dictionary<Key, Value> dict) => string.Join(", ", dict.Select(x => $"{x.Key} {x.Value}"));
        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
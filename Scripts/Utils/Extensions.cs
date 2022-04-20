using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace GodotModules 
{
    public static class Extensions 
    {
        public static string Print<T>(this List<T> list) => string.Join(", ", list);
        public static string Print<TKey, TValue>(this Dictionary<TKey, TValue> dict) => string.Join(", ", dict.Select(x => $"{x.Key} {x.Value}"));
        public static string AddSpaceBeforeEachCapital(this string str) => string.Concat(str.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        public static string ToTitleCase(this string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
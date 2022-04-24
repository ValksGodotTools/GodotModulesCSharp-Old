using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GodotModules 
{
    public static class Extensions 
    {
        public static string Print<T>(this List<T> value) => string.Join(", ", value);
        public static string Print<TKey, TValue>(this Dictionary<TKey, TValue> value) => string.Join(", ", value);
        public static string AddSpaceBeforeEachCapital(this string value) => string.Concat(value.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        public static string ToTitleCase(this string value) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        public static bool IsMatch(this string value, string expression) => Regex.IsMatch(value, expression);
        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

        public static void Validate(this string value, ref string previousValue, LineEdit input, Func<bool> condition) 
        {
            if (value.Empty()) 
            {
                input.Text = "";
                return;
            }
            
            if (!condition())
            {
                input.Text = previousValue;
                input.CaretPosition = value.Length;
                return;
            }

            previousValue = value;
        }
    }
}
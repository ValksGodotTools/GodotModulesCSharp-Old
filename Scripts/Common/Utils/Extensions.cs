using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GodotModules
{
    public static class Extensions
    {
        public static bool Duplicate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string path = null)
        {
            if (dict.ContainsKey(key))
            {
                GM.LogWarning($"'{caller}' tried to add duplicate key '{key}' to dictionary\n" +
                    $"   at {path} line:{lineNumber}");
                return true;
            }

            return false;
        }

        public static bool DoesNotHave<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string path = null)
        {
            if (!dict.ContainsKey(key))
            {
                GM.LogWarning($"'{caller}' tried to access non-existent key '{key}' from dictionary\n" +
                    $"   at {path} line:{lineNumber}");
                return true;
            }

            return false;
        }

        public static string Print<T>(this IEnumerable<T> value, bool newLine = true)
        {
            if (value != null)
                return string.Join(newLine ? "\n" : ", ", value);
            else
                return null;
        }

        public static string AddSpaceBeforeEachCapital(this string value) => string.Concat(value.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        public static string ToTitleCase(this string value) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());

        public static string SmallWordsToUpper(this string text, int maxLength = 2, Func<string, bool> filter = null)
        {
            var words = text.Split(' ');

            if (filter != null)
            {
                for (int i = 0; i < words.Length; i++)
                    if (filter(words[i]))
                        if (words[i].Length <= maxLength)
                            words[i] = words[i].ToUpper();
            }
            else
                for (int j = 0; j < words.Length; j++)
                    if (words[j].Length <= maxLength)
                        words[j] = words[j].ToUpper();

            return string.Join(" ", words);
        }

        public static bool IsMatch(this string value, string expression) => Regex.IsMatch(value, expression);

        public static bool IsNum(this string value) => int.TryParse(value, out int _);

        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T element in value)
                action(element);
        }
    }
}
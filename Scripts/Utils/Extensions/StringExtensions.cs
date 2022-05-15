using System.Globalization;
using System.Text.RegularExpressions;

namespace GodotModules 
{
    public static class StringExtensions 
    {
        public static string AddSpaceBeforeEachCapital(this string value) => string.Concat(value.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        public static string ToTitleCase(this string value) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());

        public static string SmallWordsToUpper(this string text, int maxLength = 2, Func<string, bool> filter = null)
        {
            var words = text.Split(' ');

            for (int i = 0; i < words.Length; i++)
                if (filter == null || filter(words[i]))
                    if (words[i].Length <= maxLength)
                        words[i] = words[i].ToUpper();

            return string.Join(" ", words);
        }

        public static bool IsMatch(this string value, string expression) => Regex.IsMatch(value, expression);

        public static bool IsNum(this string value) => int.TryParse(value, out int _);
    }
}
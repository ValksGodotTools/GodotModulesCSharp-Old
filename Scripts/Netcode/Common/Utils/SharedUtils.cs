using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Common.Utils
{
    public static class SharedUtils
    {
        public static string AddSpaceBeforeEachCapital(string str) => string.Concat(str.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

        public static string ToTitleCase(this string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }
    }
}

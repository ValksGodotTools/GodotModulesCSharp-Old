namespace GodotModules 
{
    public static class CollectionExtensions 
    {
        public static string Print<T>(this IEnumerable<T> value, bool newLine = true)
        {
            if (value != null)
                return string.Join(newLine ? "\n" : ", ", value);
            else
                return null;
        }

        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T element in value)
                action(element);
        }
        
        public static bool Duplicate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string path = null)
        {
            if (dict.ContainsKey(key))
            {
                Logger.LogWarning($"'{caller}' tried to add duplicate key '{key}' to dictionary\n" +
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
                Logger.LogWarning($"'{caller}' tried to access non-existent key '{key}' from dictionary\n" +
                    $"   at {path} line:{lineNumber}");
                return true;
            }

            return false;
        }
    }
}
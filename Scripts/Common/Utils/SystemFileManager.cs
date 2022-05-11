using Newtonsoft.Json;
using System.IO;

namespace GodotModules
{
    public static class SystemFileManager
    {
        public static char Separator => Path.DirectorySeparatorChar;

        public static T WriteConfig<T>(string path) where T : new() => WriteConfig<T>(path, new T());

        public static T WriteConfig<T>(string path, T data)
        {
            var contents = JsonConvert.SerializeObject(data, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, contents);
            return data;
        }

        public static T GetConfig<T>(string path)
        {
            if (!File.Exists(path))
                return default(T);

            string contents = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(contents);
        }
    }

    public enum FileStatus
    {
        DoesNotExist,
        Exists
    }
}
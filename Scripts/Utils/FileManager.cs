using Newtonsoft.Json;
using System;
using System.IO;

namespace GodotModules
{
    public static class FileManager
    {
        public static char Separator => Path.DirectorySeparatorChar;

        public static string GetGameDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GameManager.GameName);

        public static string GetProjectPath()
        {
            string pathExeDir;

            if (Godot.OS.HasFeature("standalone")) // check if game is exported
                // set to exported release dir
                pathExeDir = $"{Directory.GetParent(Godot.OS.GetExecutablePath()).FullName}";
            else
                // set to project dir
                pathExeDir = Godot.ProjectSettings.GlobalizePath("res://");

            return pathExeDir;
        }

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
using Newtonsoft.Json;
using System.IO;

namespace GodotModules
{
    public class SystemFileManager
    {
        public readonly string GameDataPath = System.IO.Path.Combine
        (
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), 
            "Godot Modules"
        );

        public readonly char Separator = Path.DirectorySeparatorChar;

        public T WriteConfig<T>(string pathToFile) where T : new() => 
            WriteConfig<T>(pathToFile, new T());

        public T WriteConfig<T>(string pathToFile, T data)
        {
            var path = GetConfigPath(pathToFile);
            var contents = JsonConvert.SerializeObject(data, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, contents);
            return data;
        }

        public T ReadConfig<T>(string pathToFile)
        {
            var path = GetConfigPath(pathToFile);
            return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) : default(T);
        }

        public bool ConfigExists(string pathToFile) => 
            File.Exists(GetConfigPath(pathToFile));

        private string GetConfigPath(string pathToFile) => 
            $"{GameDataPath}{Separator}{pathToFile}.json";
    }
}
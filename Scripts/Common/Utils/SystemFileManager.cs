using Newtonsoft.Json;
using System.IO;

namespace GodotModules
{
    public class SystemFileManager
    {
        private string _gameDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Godot Modules");

        public T WriteConfig<T>(string pathToFile) where T : new() => WriteConfig<T>(pathToFile, new T());

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

            if (!File.Exists(path))
                return default(T);

            string contents = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(contents);
        }

        public bool ConfigExists(string pathToFile) => File.Exists(GetConfigPath(pathToFile));
        private string GetConfigPath(string pathToFile) => $"{_gameDataPath}{Path.DirectorySeparatorChar}{pathToFile}.json";
    }
}
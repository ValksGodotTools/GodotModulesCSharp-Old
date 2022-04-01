using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MoonSharp.Interpreter;

namespace Game 
{
    public class ModLoader 
    {
        public static List<ModInfo> Mods = new List<ModInfo>();
        private static char Separator = Path.DirectorySeparatorChar;

        public static void Load()
        {
            string pathMods;

            if (Godot.OS.HasFeature("standalone")) // check if game is running in an exported release
                pathMods = $"{Directory.GetParent(Godot.OS.GetExecutablePath()).FullName}{Separator}Mods";
            else
                pathMods = $"C:{Separator}Mods"; // for testing so do not have to export game on every test

            if (!Directory.Exists(pathMods))
                Directory.CreateDirectory(pathMods);

            var mods = Directory.GetDirectories(pathMods);

            foreach (var mod in mods)
            {
                var files = Directory.GetFiles(mod);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);

                    if (fileName == "info.json")
                    {
                        var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(file));
                        Mods.Add(modInfo);
                    }

                    if (fileName == "script.lua") 
                    {
                        DynValue res = Script.RunFile(file);
                        Godot.GD.Print(res.Number);
                    }
                }
            }
        }
    }

    public struct ModInfo 
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
    }
}
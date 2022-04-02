using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

namespace Game
{
    public class ModLoader
    {
        public static List<ModInfo> Mods = new List<ModInfo>();

        private static Script ModScript { get; set; } // the script every mod will use

        private static char Separator = Path.DirectorySeparatorChar;

        public static void InitModScript()
        {
            // Debug server
            var server = new MoonSharpVsCodeDebugServer();
            server.Start();

            ModScript = new Script();

            server.AttachToScript(ModScript, "ModScript");

            UserData.RegisterType<Player>();
            UserData.RegisterType<Master>();

            var master = new Master();

            ModScript.Globals["Master"] = master;

            master.QueueFree();
        }

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
                        try
                        {
                            var res = ModScript.DoFile(file);
                            Godot.GD.Print(res.Number);
                        }
                        catch (ScriptRuntimeException e)
                        {
                            Godot.GD.Print(e.DecoratedMessage);
                        }

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
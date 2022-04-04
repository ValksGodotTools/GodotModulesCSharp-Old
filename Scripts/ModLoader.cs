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

        public static void Init()
        {
            string pathExeDir;

            if (Godot.OS.HasFeature("standalone")) // check if game is exported
                // set to exported release dir
                pathExeDir = $"{Directory.GetParent(Godot.OS.GetExecutablePath()).FullName}";
            else
                // set to project dir
                pathExeDir = Godot.ProjectSettings.GlobalizePath("res://");;

            
            // Debug server - not sure how this works?
            var server = new MoonSharpVsCodeDebugServer();
            server.Start();

            ModScript = new Script();

            string pathMods = Path.Combine(pathExeDir, "Mods");

            var luaPlayer = new Godot.File();
            luaPlayer.Open("res://Scripts/Lua/Player.lua", Godot.File.ModeFlags.Read);

            ModScript.DoString(luaPlayer.GetAsText());

            server.AttachToScript(ModScript, "ModScript");


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
                            ModScript.DoFile(file);

                            DynValue resPlayer = ModScript.Globals.Get("Player");
                            if (resPlayer != null)
                            {
                                var player = resPlayer.Table;
                                var resHealth = player.Get("health");

                                if (resHealth != null)
                                    Master.Player.SetHealth((int)resHealth.Number);
                            }
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
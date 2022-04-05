using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

namespace Game
{
    public class ModLoader
    {
        private static Dictionary<string, Mod> Mods = new Dictionary<string, Mod>();
        private static MoonSharpVsCodeDebugServer DebugServer { get; set; }
        private static string PathMods { get; set; }
        private static Script Script { get; set; }


        public static void Init()
        {
            FindModsPath();
            
            DebugServer = new MoonSharpVsCodeDebugServer(); // how does this work in action?
            DebugServer.Start();

            Script = new Script();

            var luaPlayer = new Godot.File();
            luaPlayer.Open("res://Scripts/Lua/Player.lua", Godot.File.ModeFlags.Read);

            Script.DoString(luaPlayer.GetAsText());

            Script.Globals["Player", "setHealth"] = (Action<int>)Master.Player.SetHealth;
        }

        public static void LoadAll()
        {
            Directory.CreateDirectory(PathMods);

            var mods = Directory.GetDirectories(PathMods);

            foreach (var mod in mods)
            {
                var files = Directory.GetFiles(mod);

                var pathInfo = $"{mod}/info.json";
                var pathScript = $"{mod}/script.lua";
                if (!File.Exists(pathInfo) || !File.Exists(pathScript))
                    continue;

                var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(pathInfo));

                try
                {
                    Script.DoFile(pathScript);

                    Mods.Add(modInfo.Name, new Mod {
                        ModInfo = modInfo
                    });

                    DebugServer.AttachToScript(Script, modInfo.Name);
                }
                catch (ScriptRuntimeException e)
                {
                    Godot.GD.Print(e.DecoratedMessage);
                }
            }
        }

        public static void Hook(string v, params object[] args)
        {
            Script.Call(Script.Globals[v], args);
        }

        private static void FindModsPath()
        {
            string pathExeDir;

            if (Godot.OS.HasFeature("standalone")) // check if game is exported
                // set to exported release dir
                pathExeDir = $"{Directory.GetParent(Godot.OS.GetExecutablePath()).FullName}";
            else
                // set to project dir
                pathExeDir = Godot.ProjectSettings.GlobalizePath("res://");

            PathMods = Path.Combine(pathExeDir, "Mods");
        }

        public static DynValue resPlayer;

        public static void UpdatePlayer()
        {
            foreach (var modName in Mods.Keys)
            {
                resPlayer = Mods[modName].Script.Globals.Get("Player");
                if (resPlayer != null)
                {
                    var player = resPlayer.Table;
                    var resHealth = player.Get("health");

                    if (resHealth != null)
                        Master.Player.SetHealth((int)resHealth.Number);
                }
            }
        }
    }

    public struct Mod 
    {
        public ModInfo ModInfo { get; set; }
        public Script Script { get; set; }
    }

    public struct ModInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
    }
}
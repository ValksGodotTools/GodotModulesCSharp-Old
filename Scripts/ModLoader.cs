using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

namespace ModLoader
{
    public class ModLoader
    {
        public static List<Mod> Mods = new List<Mod>();
        public static Dictionary<string, bool> ModsEnabled = new Dictionary<string, bool>();
        private static MoonSharpVsCodeDebugServer DebugServer { get; set; }
        public static string PathModsEnabled { get; set; }
        private static string PathMods { get; set; }
        private static Script Script { get; set; }


        public static void Init()
        {
            PathMods = FindModsPath();
            PathModsEnabled = Path.Combine(PathMods, "enabled.json");

            SetupModsEnabled();
            GetModsEnabled();

            DebugServer = new MoonSharpVsCodeDebugServer();
            DebugServer.Start();
        }

        public static void LoadGameDefs()
        {
            Script.Globals["Player", "setHealth"] = (Action<int>)Master.Player.SetHealth;
        }

        public static void SortMods()
        {
            Mods.Clear();
            var foundMods = FindAllMods();

            foreach (var mod in foundMods.Values)
            {
                if (!Mods.Contains(mod))
                    Mods.Add(mod);

                var modIndex = Mods.IndexOf(mod);

                foreach (var dependency in mod.ModInfo.Dependencies)
                {
                    if (!foundMods.ContainsKey(dependency))
                    {
                        Godot.GD.Print($"{mod.ModInfo.Name} is missing the dependency: {dependency}");
                        continue;
                    }

                    if (!Mods.Contains(foundMods[dependency]))
                        Mods.Insert(modIndex, foundMods[dependency]);
                }
            }
        }

        public static void LoadMods()
        {
            if (Script != null) 
                DebugServer.Detach(Script);

            Script = new Script();
            DebugServer.AttachToScript(Script, "Debug");

            var luaGame = new Godot.File();
            luaGame.Open("res://Scripts/Lua/Game.lua", Godot.File.ModeFlags.Read);

            Script.DoString(luaGame.GetAsText());

            var modsEnabled = ModLoader.ModsEnabled;
            
            foreach (var mod in Mods)
            {
                if (!modsEnabled[mod.ModInfo.Name])
                    continue;

                try
                {
                    Script.DoFile(mod.PathScript);
                }
                catch (ScriptRuntimeException e)
                {
                    // Mod script did not run right
                    Godot.GD.Print(e.DecoratedMessage);
                    continue;
                }
            }
        }

        public static void Call(string v, params object[] args)
        {
            try
            {
                Script.Call(Script.Globals[v], args);
            }
            catch (ScriptRuntimeException e)
            {
                Godot.GD.Print(e.DecoratedMessage);
            }
        }

        public static void SetModsEnabled()
        {
            File.WriteAllText(PathModsEnabled, JsonConvert.SerializeObject(ModsEnabled, Formatting.Indented));
        }

        public static void GetModsEnabled() 
        {
            if (!File.Exists(PathModsEnabled))
                return;
            
            ModsEnabled = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(PathModsEnabled));
        }

        private static void SetupModsEnabled()
        {
            if (File.Exists(PathModsEnabled))
                return;

            File.WriteAllText(PathModsEnabled, "{}");
        }

        private static Dictionary<string, Mod> FindAllMods()
        {
            var mods = new Dictionary<string, Mod>();

            Directory.CreateDirectory(PathMods);

            var modFolders = Directory.GetDirectories(PathMods);

            foreach (var modFolder in modFolders)
            {
                var files = Directory.GetFiles(modFolder);

                var pathInfo = $"{modFolder}/info.json";
                var pathScript = $"{modFolder}/script.lua";

                // info.json or script.lua does not exist
                if (!File.Exists(pathInfo) || !File.Exists(pathScript))
                    continue;

                var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(pathInfo));

                // info.json does not have a name field
                if (modInfo.Name == null)
                    continue;

                if (modInfo.Version == null)
                    modInfo.Version = "0.0.1";

                if (modInfo.Author == null)
                    modInfo.Author = "";

                if (modInfo.Description == null)
                    modInfo.Description = "";

                // Mod with this name exists already
                if (mods.ContainsKey(modInfo.Name))
                    continue;

                mods.Add(modInfo.Name, new Mod
                {
                    ModInfo = modInfo,
                    PathScript = pathScript
                });
            }

            return mods;
        }

        private static string FindModsPath()
        {
            string pathExeDir;

            if (Godot.OS.HasFeature("standalone")) // check if game is exported
                // set to exported release dir
                pathExeDir = $"{Directory.GetParent(Godot.OS.GetExecutablePath()).FullName}";
            else
                // set to project dir
                pathExeDir = Godot.ProjectSettings.GlobalizePath("res://");

            return Path.Combine(pathExeDir, "Mods");
        }
    }

    public struct Mod
    {
        public ModInfo ModInfo { get; set; }
        public string PathScript { get; set; }
    }

    public struct ModInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string[] GameVersions { get; set; }
        public string[] Dependencies { get; set; }
    }
}
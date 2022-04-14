using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

namespace Valk.Modules.ModLoader
{
    public class ModLoader
    {
        public static List<Mod> LoadedMods = new List<Mod>();
        public static Dictionary<string, Mod> ModInfo = new Dictionary<string, Mod>();
        public static Dictionary<string, bool> ModsEnabled = new Dictionary<string, bool>();
        private static MoonSharpVsCodeDebugServer DebugServer { get; set; }
        public static string PathModsEnabled { get; set; }
        private static string PathMods { get; set; }
        public static Script Script { get; set; }
        public static string LuaScriptsPath = "Scripts/Lua";
        public static string ModsProjectPath = "";


        public static void Init()
        {
            PathMods = Path.Combine(FileManager.GetGameDataPath(), "Mods");
            PathModsEnabled = Path.Combine(PathMods, "enabled.json");

            Directory.CreateDirectory(PathMods);
            Directory.CreateDirectory(LuaScriptsPath);

            SetupModsEnabled();
            GetModsEnabled();

            DebugServer = new MoonSharpVsCodeDebugServer();
            DebugServer.Start();

            SortMods();
        }

        public static void SortMods()
        {
            LoadedMods.Clear();
            ModInfo = FindAllMods();

            foreach (var mod in ModInfo.Values)
            {
                if (!LoadedMods.Contains(mod))
                    LoadedMods.Add(mod);

                var modIndex = LoadedMods.IndexOf(mod);

                foreach (var dependency in mod.ModInfo.Dependencies)
                {
                    if (!ModInfo.ContainsKey(dependency))
                    {
                        Log($"{mod.ModInfo.Name} is missing the dependency: {dependency}");
                        continue;
                    }

                    if (!LoadedMods.Contains(ModInfo[dependency]))
                        LoadedMods.Insert(modIndex, ModInfo[dependency]);
                }
            }
        }

        public static void LoadMods()
        {
            UIModLoader.ClearLog();

            if (Script != null) 
                DebugServer.Detach(Script);

            Script = new Script();
            DebugServer.AttachToScript(Script, "Debug");

            LoadLuaScripts(LuaScriptsPath);

            var modsEnabled = ModLoader.ModsEnabled;
            var modLoadedCount = 0;
            
            foreach (var mod in LoadedMods)
            {
                // if mod is not enabled do not run it
                if (!modsEnabled[mod.ModInfo.Name])
                    continue;

                var allDependenciesEnabled = true;

                // check if mods dependencies are enabled
                foreach (var dependency in mod.ModInfo.Dependencies)
                {
                    if (!modsEnabled[dependency])
                    {
                        Log($"{mod.ModInfo.Name} requires dependency {dependency} to be enabled");
                        allDependenciesEnabled = false;
                    }
                }

                // do not load mod if not all dependencies are loaded
                if (!allDependenciesEnabled)
                    continue;

                // load the mod
                modLoadedCount++;

                try
                {
                    Script.DoFile(mod.PathScript);
                    UIModLoader.Log($"Loaded {mod.ModInfo.Name}");
                }
                catch (ScriptRuntimeException e)
                {
                    // Mod script did not run right
                    LogErr(e.DecoratedMessage);
                    continue;
                }
            }

            if (modLoadedCount > 0)
                UIModLoader.Log($"{modLoadedCount} mods have loaded successfully");
        }

        public static void Call(string v, params object[] args)
        {
            try
            {
                Script.Call(Script.Globals[v], args);
            }
            catch (ScriptRuntimeException e)
            {
                LogErr(e.DecoratedMessage);
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

        private static void Log(object obj) 
        {
            Godot.GD.Print($"[ModLoader]: {obj}");
            UIModLoader.Log($"{obj}");
        }

        private static void LogErr(object obj) 
        {
            Godot.GD.PrintErr($"[ModLoader]: {obj}");
            UIModLoader.Log($"{obj}");
        }

        private static void LoadLuaScripts(string directory)
        {
            var luaDir = new Godot.Directory();
            if (luaDir.Open($"res://{directory}") != Godot.Error.Ok)
            {
                LogErr("Could not open lua directory");
                return;
            }

            luaDir.ListDirBegin(true);
            var fileName = luaDir.GetNext();
            while (fileName != "")
            {
                var absolutePath = $"{directory}/{fileName}";

                if (luaDir.CurrentIsDir())
                    LoadLuaScripts(absolutePath);
                else
                {
                    var luaScript = new Godot.File();
                    var err = luaScript.Open(absolutePath, Godot.File.ModeFlags.Read);

                    if (err == Godot.Error.Ok)
                        Script.DoString(luaScript.GetAsText());
                    else
                        LogErr($"Could not open file: {absolutePath}");
                }

                fileName = luaDir.GetNext();
            }
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
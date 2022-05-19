using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using Newtonsoft.Json;
using System.IO;

namespace GodotModules
{
    public static class ModLoader
    {
        public static SceneMods SceneMods { get; set; }
        public static string ModLoaderLogs { get; set; }
        public static string PathModsFolder { get; private set; }
        public static Dictionary<string, Mod> Mods { get; private set; }
        public static Script Script { get; set; }

        private static string _pathModsEnabled { get; set; }
        private static string _pathLuaScripts { get; set; }
        private static SystemFileManager _systemFileManager { get; set; }
        private static GodotFileManager _godotFileManager { get; set; }

        public static void Init(SystemFileManager systemFileManager, GodotFileManager godotFileManager)
        {
            ModLoaderLogs = "";
            PathModsFolder = Path.Combine(systemFileManager.GameDataPath, "Mods");
            _pathModsEnabled = Path.Combine(PathModsFolder, "enabled.json");
            _pathLuaScripts = Path.Combine("Scripts", "Lua");
            _systemFileManager = systemFileManager;
            _godotFileManager = godotFileManager;

            Directory.CreateDirectory(PathModsFolder);

            if (!_systemFileManager.ConfigExists(Path.Combine("Mods", "enabled")))
                _systemFileManager.WriteConfig(Path.Combine("Mods", "enabled"), new Dictionary<string, bool>());

            GetMods();
            LoadMods();
        }

        public static void Call(string v, params object[] args)
        {
            try
            {
                Script.Call(Script.Globals[v], args);
            }
            catch (ScriptRuntimeException e)
            {
                Logger.LogErr(e, $"[ModLoader]: ");
            }
        }

        public static void GetMods() => Mods = GetModData();

        public static void LoadMods()
        {
            foreach (var mod in Mods.Values)
                mod.ModInfo.MissingDependencies.Clear();

            Script = new Script();
            LoadLuaScripts(_pathLuaScripts);

            DisableModsWithLackingDependencies();

            var loadedModCount = 0;

            foreach (var mod in Mods.Values) 
            {
                if (mod.ModInfo.MissingDependencies.Count > 0) 
                {
                    Log($"{mod.ModInfo.Name} is missing dependencies: {mod.ModInfo.MissingDependencies.Print(false)}");
                    continue;
                }

                if (mod.ModInfo.Enabled)
                    LoadMod(mod, ref loadedModCount);
            }

            Log(loadedModCount != 0 ? $"{loadedModCount} mods have loaded successfully" : "No mods have been loaded");
        }

        public static void EnableMod(string name) 
        {
            if (Mods.ContainsKey(name))
                Mods[name].ModInfo.Enabled = true;
        }

        public static void DisableMod(string name) 
        {
            if (Mods.ContainsKey(name))
                Mods[name].ModInfo.Enabled = false;
        }

        public static void SaveEnabled() =>
            _systemFileManager.WriteConfig(Path.Combine("Mods", "enabled"), Mods.ToDictionary(x => x.Key, x => x.Value.ModInfo.Enabled));

        private static void Log(string message)
        {
            ModLoaderLogs += $"{message}\n";
            SceneMods?.Log(message);
            Logger.Log(message);
        }
        
        private static void DisableModsWithLackingDependencies()
        {
            foreach (var pair in Mods)
            {
                var mod = pair.Value;

                if (mod.ModInfo.Enabled)
                    CheckAllDependenciesEnabled(mod.ModInfo, mod.ModInfo, new List<string> { pair.Key });
            }
        }

        private static void CheckAllDependenciesEnabled(ModInfo firstMod, ModInfo modInfo, List<string> checkedMods)
        {
            foreach (var dependency in modInfo.Dependencies)
            {
                if (checkedMods.Contains(dependency))
                    continue;

                if (!Mods.ContainsKey(dependency))
                    Mods[firstMod.Name].ModInfo.MissingDependencies.Add(dependency);
                else if (!Mods[dependency].ModInfo.Enabled)
                    Mods[firstMod.Name].ModInfo.MissingDependencies.Add(dependency);

                checkedMods.Add(dependency);

                if (Mods.ContainsKey(dependency))
                    CheckAllDependenciesEnabled(firstMod, Mods[dependency].ModInfo, checkedMods);
            }
        }

        private static void LoadMod(Mod mod, ref int modLoadedCount)
        {
            try
            {
                Script.DoFile(mod.PathScript);
                Log($"Loaded {mod.ModInfo.Name}");
                modLoadedCount++;
            }
            catch (ScriptRuntimeException e)
            {
                Log($"Failed to load {mod.ModInfo.Name}");
                Logger.LogErr(e, "[ModLoader]: ");
            }
        }

        private static Dictionary<string, Mod> GetModData()
        {
            var mods = new Dictionary<string, Mod>();

            // retreive mod data
            foreach (var modDirectory in Directory.GetDirectories(PathModsFolder))
            {
                var pathInfo = Path.Combine(modDirectory, "info.json");
                var pathScript = Path.Combine(modDirectory, "script.lua");

                if (!File.Exists(pathInfo) || !File.Exists(pathScript))
                    continue;

                var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(pathInfo));

                if (modInfo.Name == null)
                    continue;

                if (mods.ContainsKey(modInfo.Name))
                    continue;

                if (modInfo.Version == null) modInfo.Version = "0.1";
                if (modInfo.GameVersions == null) modInfo.GameVersions = new string[] { "0.1" };
                if (modInfo.Dependencies == null) modInfo.Dependencies = new string[] { };
                if (modInfo.Author == null) modInfo.Author = "";
                if (modInfo.Description == null) modInfo.Description = "";

                mods.Add(modInfo.Name, new Mod(modInfo, pathScript));
            }

            // retreive mods enabled
            var modsEnabled = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(_pathModsEnabled));

            foreach (var mod in modsEnabled)
                if (mods.ContainsKey(mod.Key))
                    mods[mod.Key].ModInfo.Enabled = mod.Value;

            // check dependencies (just checking if the dependency exists, not if it's enabled or not)
            foreach (var mod in mods.Values)
                foreach (var dependency in mod.ModInfo.Dependencies)
                    if (!mods.ContainsKey(dependency))
                        mod.ModInfo.MissingDependencies.Add(dependency);

            return mods;
        }

        private static void LoadLuaScripts(string directory)
        {
            _godotFileManager.LoadDir(directory, (dir, fileName) =>
            {
                var absolutePath = $"{dir.GetCurrentDir()}/{fileName}";

                if (dir.CurrentIsDir())
                    LoadLuaScripts(absolutePath);
                else
                {
                    var luaScript = new Godot.File();
                    var err = luaScript.Open(absolutePath, Godot.File.ModeFlags.Read);

                    if (err == Godot.Error.Ok)
                        Script.DoString(luaScript.GetAsText());
                    else
                        Log($"Could not open file: {absolutePath}");
                }
            });
        }
    }

    public class Mod
    {
        public ModInfo ModInfo { get; set; }
        public string PathScript { get; set; }

        public Mod(ModInfo modInfo, string pathScript)
        {
            ModInfo = modInfo;
            PathScript = pathScript;
        }
    }

    public class ModInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string[] GameVersions { get; set; }
        public string[] Dependencies { get; set; }
        public List<string> MissingDependencies = new List<string>();
        public bool Enabled { get; set; }
    }
}
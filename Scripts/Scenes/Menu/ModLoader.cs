using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GodotModules.ModLoader
{
    public class ModLoader
    {
        public static List<Mod> LoadedMods = new List<Mod>();
        public static Dictionary<string, Mod> ModInfo = new Dictionary<string, Mod>();
        private static MoonSharpVsCodeDebugServer DebugServer { get; set; }
        public static string PathModsEnabled { get; set; }
        private static string PathMods { get; set; }
        public static Script Script { get; set; }
        public static string LuaScriptsPath = "";

        public static void Init()
        {
            PathMods = Path.Combine(SystemFileManager.GetGameDataPath(), "Mods");
            PathModsEnabled = Path.Combine(PathMods, "enabled.json");

            LuaScriptsPath = $"Scripts{SystemFileManager.Separator}Lua";

            Directory.CreateDirectory(PathMods);

            SetupModsEnabled();

            //DebugServer = new MoonSharpVsCodeDebugServer();
            //DebugServer.Start();

            SortMods();
        }

        public static void SortMods()
        {
            LoadedMods.Clear();
            ModInfo = FindAllMods();
            GetModsEnabled();

            ModInfo.Values.ForEach(mod =>
            {
                if (!LoadedMods.Contains(mod))
                    LoadedMods.Add(mod);

                var modInfo = mod.ModInfo;
                var modIndex = LoadedMods.IndexOf(mod);

                modInfo.Dependencies
                    .Where(IsModMissingDependency)
                    .ForEach(x => Log($"{mod.ModInfo.Name} is missing the dependency: {x}"));

                modInfo.Dependencies
                    .Where(ModHasDependency)
                    .ForEach(x => AddDependencyToLoadedMods(modIndex, x));
            });
        }

        public static void LoadMods()
        {
            UIModLoader.Instance.ClearLog();

            //if (Script != null)
            //DebugServer.Detach(Script);

            Script = new Script();
            //DebugServer.AttachToScript(Script, "Debug");

            LoadLuaScripts(LuaScriptsPath);

            var modLoadedCount = 0;

            // check dependencies
            //LoadedMods.ForEach(x => DisableModsWithLackingDependencies(x, x, new List<string> { x.ModInfo.Name }));
            DisableModsWithLackingDependencies();

            LoadedMods
                .Where(x => x.ModInfo.Enabled)
                .ForEach(mod => LoadMod(mod, ref modLoadedCount));

            if (modLoadedCount > 0)
                UIModLoader.Instance.Log($"{modLoadedCount} mods have loaded successfully");
        }

        private static void DisableModsWithLackingDependencies()
        {
            var modsToDisable = new List<ModInfo>();

            foreach (var pair in ModInfo)
            {
                var mod = pair.Value;

                if (pair.Value.ModInfo.Enabled)
                    CheckAllDependenciesEnabled(mod.ModInfo, mod.ModInfo, new List<string> { pair.Key }, ref modsToDisable);
            }

            foreach (var mod in modsToDisable)
                mod.Enabled = false;
        }

        private static void CheckAllDependenciesEnabled(ModInfo firstMod, ModInfo modInfo, List<string> checkedMods, ref List<ModInfo> modsToDisable) 
        {
            foreach (var dependency in modInfo.Dependencies) 
            {
                if (checkedMods.Contains(dependency))
                    continue;

                if (!ModInfo[dependency].ModInfo.Enabled) 
                {
                    if (!modsToDisable.Contains(firstMod))
                        modsToDisable.Add(firstMod);
                    UIModLoader.Instance.Log($"{firstMod.Name} requires dependency {dependency} to be enabled");
                }
                checkedMods.Add(dependency);
                CheckAllDependenciesEnabled(firstMod, ModInfo[dependency].ModInfo, checkedMods, ref modsToDisable);
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
                UIModLoader.Instance.Log($"{e.DecoratedMessage}");
                Utils.LogErr($"[ModLoader]: {e}");
            }
        }

        public static void SetModsEnabled() 
        {
            var modsEnabled = new Dictionary<string, bool>();
            foreach (var mod in ModInfo)
                modsEnabled.Add(mod.Key, mod.Value.ModInfo.Enabled);
            
            File.WriteAllText(PathModsEnabled, JsonConvert.SerializeObject(modsEnabled, Formatting.Indented));
        }

        public static void GetModsEnabled()
        {
            if (!File.Exists(PathModsEnabled))
                return;

            var modsEnabled = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(PathModsEnabled));

            foreach (var mod in modsEnabled) 
            {
                ModInfo[mod.Key].ModInfo.Enabled = mod.Value;
            }
        }

        private static void Log(object obj)
        {
            Godot.GD.Print($"[ModLoader]: {obj}");
            UIModLoader.Instance.Log($"{obj}");
        }

        private static void LoadLuaScripts(string directory)
        {
            GodotFileManager.LoadDir(directory, (dir, fileName) =>
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
                    {
                        UIModLoader.Instance.Log($"Could not open file: {absolutePath}");
                        Utils.LogErr($"Could not open file: {absolutePath}");
                    }
                }
            });
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
                var pathInfo = $"{modFolder}/info.json";
                var pathScript = $"{modFolder}/script.lua";

                if (!RequiredModFilesExist(modFolder, pathInfo, pathScript))
                    continue;

                var modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(pathInfo));

                if (!ModHasName(modInfo))
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

                modInfo.Enabled = true;

                mods.Add(modInfo.Name, new Mod
                {
                    ModInfo = modInfo,
                    PathScript = pathScript
                });
            }

            return mods;
        }

        private static void AddDependencyToLoadedMods(int modIndex, string dependency)
        {
            if (!LoadedMods.Contains(ModInfo[dependency]))
                LoadedMods.Insert(modIndex, ModInfo[dependency]);
        }

        private static void LoadMod(Mod mod, ref int modLoadedCount)
        {
            try
            {
                Script.DoFile(mod.PathScript);
                UIModLoader.Instance.Log($"Loaded {mod.ModInfo.Name}");
                modLoadedCount++;
            }
            catch (ScriptRuntimeException e)
            {
                // Mod script did not run right
                UIModLoader.Instance.Log($"{e.DecoratedMessage}");
                Utils.LogErr($"[ModLoader]: {e}");
            }
        }

        private static bool ModHasName(ModInfo modInfo) => modInfo.Name != null;
        private static bool RequiredModFilesExist(string folder, string pathInfo, string pathScriptLua) => File.Exists(pathInfo) && File.Exists(pathScriptLua);
        private static bool IsModMissingDependency(string dependency) => !ModInfo.ContainsKey(dependency);
        private static bool ModHasDependency(string dependency) => ModInfo.ContainsKey(dependency);
        private static bool ModHasAllDependenciesEnabled(Mod mod)
        {
            var allDependenciesEnabled = true;

            mod.ModInfo.Dependencies
                .Where(x => !ModInfo[x].ModInfo.Enabled)
                .ForEach(dependency =>
                {
                    Log($"{mod.ModInfo.Name} requires dependency {dependency} to be enabled");
                    allDependenciesEnabled = false;
                });

            return allDependenciesEnabled;
        }
    }

    public class Mod
    {
        public ModInfo ModInfo { get; set; }
        public string PathScript { get; set; }
    }

    public class ModInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string[] GameVersions { get; set; }
        public string[] Dependencies { get; set; }
        public bool Enabled { get; set; }
    }
}
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
        public static Dictionary<string, bool> ModsEnabled = new Dictionary<string, bool>();
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
            GetModsEnabled();

            //DebugServer = new MoonSharpVsCodeDebugServer();
            //DebugServer.Start();

            SortMods();
        }

        public static void SortMods()
        {
            LoadedMods.Clear();
            ModInfo = FindAllMods();

            ModInfo.Values.ForEach(mod =>
            {
                if (!LoadedMods.Contains(mod))
                    LoadedMods.Add(mod);

                var modIndex = LoadedMods.IndexOf(mod);

                mod.ModInfo.Dependencies
                    .Where(dependency =>
                    {
                        if (!ModInfo.ContainsKey(dependency))
                        {
                            Log($"{mod.ModInfo.Name} is missing the dependency: {dependency}");
                            return false;
                        }
                        else
                            return true;
                    }).ForEach(dependency =>
                    {
                        if (!LoadedMods.Contains(ModInfo[dependency]))
                            LoadedMods.Insert(modIndex, ModInfo[dependency]);
                    });
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

            var modsEnabled = ModLoader.ModsEnabled;
            var modLoadedCount = 0;

            bool ModIsEnabled(Mod mod) => modsEnabled[mod.ModInfo.Name];
            bool ModHasAllDependenciesEnabled(Mod mod)
            {
                var allDependenciesEnabled = true;

                mod.ModInfo.Dependencies.Where(dependency => !modsEnabled[dependency]).ForEach(dependency => {
                    Log($"{mod.ModInfo.Name} requires dependency {dependency} to be enabled");
                    allDependenciesEnabled = false;
                });

                return allDependenciesEnabled;
            }

            LoadedMods
                .Where(ModIsEnabled)
                .Where(ModHasAllDependenciesEnabled)
                .ForEach(mod =>
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
                });

            if (modLoadedCount > 0)
                UIModLoader.Instance.Log($"{modLoadedCount} mods have loaded successfully");
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

        public static void SetModsEnabled() => File.WriteAllText(PathModsEnabled, JsonConvert.SerializeObject(ModsEnabled, Formatting.Indented));

        public static void GetModsEnabled()
        {
            if (!File.Exists(PathModsEnabled))
                return;

            ModsEnabled = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(PathModsEnabled));
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
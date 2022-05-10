using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using Newtonsoft.Json;

using System.IO;

namespace GodotModules
{
    public class ModLoader
    {
        public List<Mod> LoadedMods = new List<Mod>();
        public Dictionary<string, Mod> ModInfo = new Dictionary<string, Mod>();
        private MoonSharpVsCodeDebugServer DebugServer { get; set; }
        public string PathModsEnabled { get; set; }
        public string PathMods { get; set; }
        public Script Script { get; set; }
        public string LuaScriptsPath = "";
        public UIModLoader UIModLoader { get; set; }

        public ModLoader(UIModLoader uiModLoader)
        {
            UIModLoader = uiModLoader;
        }

        public void Init()
        {
            PathMods = Path.Combine(GM.GetGameDataPath(), "Mods");
            PathModsEnabled = Path.Combine(PathMods, "enabled.json");

            LuaScriptsPath = $"Scripts{SystemFileManager.Separator}Lua";

            Directory.CreateDirectory(PathMods);

            SetupModsEnabled();

            //DebugServer = new MoonSharpVsCodeDebugServer();
            //DebugServer.Start();

            SortMods();
        }

        public void SortMods()
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

        public void LoadMods()
        {
            UIModLoader.ClearLog();

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
                UIModLoader.Log($"{modLoadedCount} mods have loaded successfully");
        }

        private void DisableModsWithLackingDependencies()
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

        private void CheckAllDependenciesEnabled(ModInfo firstMod, ModInfo modInfo, List<string> checkedMods, ref List<ModInfo> modsToDisable)
        {
            foreach (var dependency in modInfo.Dependencies)
            {
                if (checkedMods.Contains(dependency))
                    continue;

                if (!ModInfo[dependency].ModInfo.Enabled)
                {
                    if (!modsToDisable.Contains(firstMod))
                        modsToDisable.Add(firstMod);
                    UIModLoader.Log($"{firstMod.Name} requires dependency {dependency} to be enabled");
                }
                checkedMods.Add(dependency);
                CheckAllDependenciesEnabled(firstMod, ModInfo[dependency].ModInfo, checkedMods, ref modsToDisable);
            }
        }

        public void Call(string v, params object[] args)
        {
            try
            {
                Script.Call(Script.Globals[v], args);
            }
            catch (ScriptRuntimeException e)
            {
                UIModLoader.Log($"{e.DecoratedMessage}");
                GM.LogErr(e, $"[ModLoader]: ");
            }
        }

        public void SetModsEnabled()
        {
            var modsEnabled = new Dictionary<string, bool>();
            foreach (var mod in ModInfo)
                modsEnabled.Add(mod.Key, mod.Value.ModInfo.Enabled);

            File.WriteAllText(PathModsEnabled, JsonConvert.SerializeObject(modsEnabled, Formatting.Indented));
        }

        public void GetModsEnabled()
        {
            if (!File.Exists(PathModsEnabled))
                return;

            var modsEnabled = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(PathModsEnabled));

            foreach (var mod in modsEnabled)
            {
                ModInfo[mod.Key].ModInfo.Enabled = mod.Value;
            }
        }

        private void Log(object obj)
        {
            GM.Log($"[ModLoader]: {obj}");
            UIModLoader.Log($"{obj}");
        }

        private void LoadLuaScripts(string directory)
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
                        UIModLoader.Log($"Could not open file: {absolutePath}");
                        GM.LogWarning($"Could not open file: {absolutePath}");
                    }
                }
            });
        }

        private void SetupModsEnabled()
        {
            if (File.Exists(PathModsEnabled))
                return;

            File.WriteAllText(PathModsEnabled, "{}");
        }

        private Dictionary<string, Mod> FindAllMods()
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

        private void AddDependencyToLoadedMods(int modIndex, string dependency)
        {
            if (!LoadedMods.Contains(ModInfo[dependency]))
                LoadedMods.Insert(modIndex, ModInfo[dependency]);
        }

        private void LoadMod(Mod mod, ref int modLoadedCount)
        {
            try
            {
                Script.DoFile(mod.PathScript);
                UIModLoader.Log($"Loaded {mod.ModInfo.Name}");
                modLoadedCount++;
            }
            catch (ScriptRuntimeException e)
            {
                // Mod script did not run right
                UIModLoader.Log($"{e.DecoratedMessage}");
                GM.LogErr(e, "[ModLoader]: ");
            }
        }

        private bool ModHasName(ModInfo modInfo) => modInfo.Name != null;

        private bool RequiredModFilesExist(string folder, string pathInfo, string pathScriptLua) => File.Exists(pathInfo) && File.Exists(pathScriptLua);

        private bool IsModMissingDependency(string dependency) => !ModInfo.ContainsKey(dependency);

        private bool ModHasDependency(string dependency) => ModInfo.ContainsKey(dependency);

        private bool ModHasAllDependenciesEnabled(Mod mod)
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
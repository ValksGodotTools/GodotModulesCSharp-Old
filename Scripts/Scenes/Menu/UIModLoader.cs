using Godot;
using System.Collections.Generic;

namespace GodotModules.ModLoader
{
    public class UIModLoader : Control
    {
        [Export] public readonly NodePath NodePathModList;
        [Export] public readonly NodePath NodePathModName;
        [Export] public readonly NodePath NodePathModGameVersions;
        [Export] public readonly NodePath NodePathModDependencies;
        [Export] public readonly NodePath NodePathModDescription;
        [Export] public readonly NodePath NodePathLogger;

        // mod list
        public static VBoxContainer ModList { get; set; } // where the ModInfo children are added

        public static Dictionary<string, UIModInfo> ModInfoList = new Dictionary<string, UIModInfo>(); // references to the ModInfo children added to the ModList VBoxContainer
        public static Dictionary<string, UIModInfo> ModInfoDependencyList = new Dictionary<string, UIModInfo>(); // refernces to the ModInfo children in the dependency list if any

        // mod info
        public static Label ModName { get; set; }

        public static Label ModGameVersions { get; set; }
        public static VBoxContainer ModDependencies { get; set; }
        public static Label ModDescription { get; set; }

        // logger
        private static RichTextLabel Logger { get; set; }

        public override void _Ready()
        {
            ModList = GetNode<VBoxContainer>(NodePathModList);
            ModName = GetNode<Label>(NodePathModName);
            ModGameVersions = GetNode<Label>(NodePathModGameVersions);
            ModDependencies = GetNode<VBoxContainer>(NodePathModDependencies);
            ModDescription = GetNode<Label>(NodePathModDescription);
            Logger = GetNode<RichTextLabel>(NodePathLogger);

            ModName.Text = "";
            ModGameVersions.Text = "";
            ModDescription.Text = "";
            Logger.Clear();

            ModLoader.Init();
            ModLoader.LoadMods();

            DisplayMods();
        }

        public static void ClearLog() => Logger.Clear();

        public static void Log(string text) => Logger.AddText($"{text}\n");

        public static void UpdateModInfo(string name)
        {
            foreach (Node node in ModDependencies.GetChildren())
                node.QueueFree();

            var modInfo = ModLoader.ModInfo[name].ModInfo;

            ModName.Text = name;

            var gameVersions = modInfo.GameVersions == null ? "" : string.Join(" ", modInfo.GameVersions);
            ModGameVersions.Text = $"Game Version(s): {gameVersions}";

            var description = modInfo.Description == null ? "" : modInfo.Description;
            ModDescription.Text = description;

            var modsEnabled = ModLoader.ModsEnabled;
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");

            ModInfoDependencyList.Clear();

            foreach (var dependency in modInfo.Dependencies)
            {
                var instance = CreateModInfoInstance(dependency);
                instance.DisplayedInDependencies = true;

                ModInfoDependencyList[dependency] = instance;

                ModDependencies.AddChild(instance);
            }
        }

        public static void DisplayMods()
        {
            var modsEnabled = ModLoader.ModsEnabled;
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
            foreach (var mod in ModLoader.LoadedMods)
            {
                var modInfo = CreateModInfoInstance(mod.ModInfo.Name);
                ModList.AddChild(modInfo);
                ModInfoList[mod.ModInfo.Name] = modInfo;
            }

            if (ModLoader.LoadedMods.Count > 0)
                UIModLoader.UpdateModInfo(ModLoader.LoadedMods[0].ModInfo.Name);
        }

        private static UIModInfo CreateModInfoInstance(string modName)
        {
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
            var instance = modInfoPrefab.Instance<UIModInfo>();
            instance.SetModName(modName);

            if (ModLoader.ModsEnabled.ContainsKey(modName))
                instance.SetModEnabled(ModLoader.ModsEnabled[modName]);
            else
                instance.SetModEnabled(false);

            return instance;
        }

        private void _on_Refresh_pressed()
        {
            ModLoader.SetModsEnabled();

            foreach (Node node in ModList.GetChildren())
                node.QueueFree();

            ModLoader.SortMods();
            UIModLoader.DisplayMods();
        }

        private void _on_Load_Mods_pressed()
        {
            ModLoader.SetModsEnabled();
            ModLoader.LoadMods();
        }
    }
}
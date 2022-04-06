using Godot;
using System;

namespace ModLoader 
{
    public class UIModLoader : Control
    {
        [Export] public readonly NodePath NodePathModList;
        [Export] public readonly NodePath NodePathModName;
        [Export] public readonly NodePath NodePathModGameVersions;
        [Export] public readonly NodePath NodePathModDependencies;
        [Export] public readonly NodePath NodePathModDescription;

        // mod list
        public static VBoxContainer ModList { get; set; }

        // mod info
        public static Label ModName { get; set; }
        public static Label ModGameVersions { get; set; }
        public static VBoxContainer ModDependencies { get; set; }
        public static Label ModDescription { get; set; }

        public override void _Ready()
        {
            ModList = GetNode<VBoxContainer>(NodePathModList);
            ModName = GetNode<Label>(NodePathModName);
            ModGameVersions = GetNode<Label>(NodePathModGameVersions);
            ModDependencies = GetNode<VBoxContainer>(NodePathModDependencies);
            ModDescription = GetNode<Label>(NodePathModDescription);
        }

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
            
            foreach (var dependency in modInfo.Dependencies)
                ModDependencies.AddChild(CreateModInfoInstance(dependency));
        }

        public static void DisplayMods()
        {
            var modsEnabled = ModLoader.ModsEnabled;
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
            foreach (var mod in ModLoader.LoadedMods)
                ModList.AddChild(CreateModInfoInstance(mod.ModInfo.Name));
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

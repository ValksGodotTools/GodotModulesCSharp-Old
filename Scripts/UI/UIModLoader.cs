using Godot;
using System;

namespace ModLoader 
{
    public class UIModLoader : Control
    {
        [Export] public readonly NodePath NodePathModList;

        private static VBoxContainer ModList;

        public override void _Ready()
        {
            ModList = GetNode<VBoxContainer>(NodePathModList);
        }

        public static void DisplayMods()
        {
            var modsEnabled = ModLoader.ModsEnabled;
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
            foreach (var mod in ModLoader.Mods)
            {
                var instance = modInfoPrefab.Instance<UIModInfo>();
                var modName = mod.ModInfo.Name;
                instance.SetModName(modName);

                if (modsEnabled.ContainsKey(modName))
                    instance.SetModEnabled(modsEnabled[modName]);
                else
                    instance.SetModEnabled(false);
                
                ModList.AddChild(instance);
            }
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

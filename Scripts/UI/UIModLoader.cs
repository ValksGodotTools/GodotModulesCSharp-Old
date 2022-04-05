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
            var modInfoPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
            foreach (var mod in ModLoader.Mods)
            {
                var instance = modInfoPrefab.Instance<UIModInfo>();
                instance.SetModName(mod.ModInfo.Name);
                ModList.AddChild(instance);
            }
        }

        private void _on_Load_Mods_pressed()
        {
            ModLoader.LoadMods();
        }
    }
}

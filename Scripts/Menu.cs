using Godot;
using System;

namespace ModLoader 
{
    public class Menu : Node
    {
        public override void _Ready()
        {
            ModLoader.Init();
            ModLoader.SortMods();
            foreach (var mod in ModLoader.Mods)
                Godot.GD.Print(mod.ModInfo.Name);
            Godot.GD.Print("------");
            UIModLoader.DisplayMods();
        }

        private void _on_Play_pressed()
        {
            GetTree().ChangeScene("res://Scenes/Main/Main.tscn");
        }
    }
}

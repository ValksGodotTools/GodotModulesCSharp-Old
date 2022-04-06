using Godot;
using System;
using System.Linq;

namespace ModLoader 
{
    public class Menu : Node
    {
        public override void _Ready()
        {
            ModLoader.Init();
            ModLoader.SortMods();
            UIModLoader.DisplayMods();
            UIModLoader.UpdateModInfo(((UIModInfo)UIModLoader.ModList.GetChild(0)).LabelModName.Text); // set to first mod in list
            ModLoader.LoadMods();
        }

        private void _on_Play_pressed()
        {
            GetTree().ChangeScene("res://Scenes/Main/Main.tscn");
        }
    }
}

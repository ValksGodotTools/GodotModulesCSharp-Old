using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        public static PackedScene UIHotkey = LoadPrefab("UIHotkey");

        private static PackedScene LoadPrefab(string prefab) => ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}
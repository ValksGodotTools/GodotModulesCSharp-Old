using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        public readonly static PackedScene UIHotkey = LoadPrefab("UIHotkey");
        public readonly static PackedScene UIErrorNotifier = LoadPrefab("UIErrorNotifier");

        private static PackedScene LoadPrefab(string prefab) => ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}
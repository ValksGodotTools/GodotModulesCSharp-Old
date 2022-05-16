using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        // Prefabs
        public readonly static PackedScene UIHotkey = LoadPrefab("UIHotkey");
        public readonly static PackedScene UIErrorNotifier = LoadPrefab("UIErrorNotifier");

        // Popups
        public readonly static PackedScene UIPopupMessage = LoadPopupPrefab("UIPopupMessage");
        public readonly static PackedScene UIPopupError = LoadPopupPrefab("UIPopupError");

        private static PackedScene LoadPopupPrefab(string popup) => LoadPrefab($"Popups/{popup}");
        private static PackedScene LoadPrefab(string prefab) => ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}
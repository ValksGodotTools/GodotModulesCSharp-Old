using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        // Game
        public static PackedScene ClientPlayer = LoadPrefab("ClientPlayer");
        public static PackedScene OtherPlayer = LoadPrefab($"OtherPlayer");

        // UI
        public static PackedScene PopupDirectConnect = LoadPrefab($"PopupDirectConnect");
        public static PackedScene PopupError = LoadPrefab("PopupError");
        public static PackedScene PopupMessage = LoadPrefab("PopupMessage");
        public static PackedScene PopupFileDialogMods = LoadPrefab("PopupFileDialogMods");
        public static PackedScene NotifyError = LoadPrefab("NotifyError");
        public static PackedScene LobbyListing = LoadPrefab("LobbyListing");
        public static PackedScene LobbyPlayerListing = LoadPrefab("LobbyPlayerListing");
        public static PackedScene ModInfo = LoadPrefab("ModInfo");

        private static PackedScene LoadPrefab(string prefab) =>
            ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}
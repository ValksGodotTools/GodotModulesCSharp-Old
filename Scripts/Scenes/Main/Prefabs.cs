using Godot;

namespace GodotModules
{
    public static class Prefabs
    {
        // Game
        public static PackedScene ClientPlayer = LoadGamePrefab("ClientPlayer");
        public static PackedScene OtherPlayer = LoadGamePrefab("OtherPlayer");
        public static PackedScene Bullet = LoadGamePrefab("Bullet");

        // Popup
        public static PackedScene PopupDirectConnect = LoadPopupPrefab($"PopupDirectConnect");
        public static PackedScene PopupError = LoadPopupPrefab("PopupError");
        public static PackedScene PopupMessage = LoadPopupPrefab("PopupMessage");
        public static PackedScene PopupFileDialogMods = LoadPopupPrefab("PopupFileDialogMods");

        // Msc
        public static PackedScene NotifyError = LoadPrefab("NotifyError");
        public static PackedScene LobbyListing = LoadPrefab("LobbyListing");
        public static PackedScene LobbyPlayerListing = LoadPrefab("LobbyPlayerListing");
        public static PackedScene ModInfo = LoadPrefab("ModInfo");


        private static PackedScene LoadPopupPrefab(string prefab) => LoadPrefab($"Popups/{prefab}");
        private static PackedScene LoadGamePrefab(string prefab) => LoadPrefab($"Game/{prefab}");
        private static PackedScene LoadPrefab(string prefab) =>
            ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}
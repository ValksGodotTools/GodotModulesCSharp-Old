using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        // UI
        public readonly static PackedScene UIHotkey = LoadUI("UIHotkey");
        public readonly static PackedScene UIErrorNotifier = LoadUI("UIErrorNotifier");
        public readonly static PackedScene UIModBtn = LoadUI("UIModBtn");
        public readonly static PackedScene UILobbyListing = LoadUI("UILobbyListing");

        // Popups
        public readonly static PackedScene PopupMessage = LoadPopup("PopupMessage");
        public readonly static PackedScene PopupError = LoadPopup("PopupError");
        public readonly static PackedScene PopupLineEdit = LoadPopup("PopupLineEdit");
        public readonly static PackedScene PopupCreateLobby = LoadPopup("PopupCreateLobby");

        // Game
        public readonly static PackedScene Player = LoadGame("Player");
        public readonly static PackedScene OtherPlayer = LoadGame("OtherPlayer");
        public readonly static PackedScene Enemy = LoadGame("Enemy");
        public readonly static PackedScene Orb = LoadGame("Orb");

        private static PackedScene LoadGame(string game) => Load($"Game/{game}");
        private static PackedScene LoadPopup(string popup) => LoadUI($"Popups/{popup}");
        private static PackedScene LoadUI(string ui) => Load($"UI/{ui}");
        private static PackedScene Load(string prefab) => ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{prefab}.tscn");
    }
}

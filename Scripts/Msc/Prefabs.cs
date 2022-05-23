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

        // 2D
        public readonly static PackedScene Player = Load2D("Player");
        public readonly static PackedScene OtherPlayer = Load2D("OtherPlayer");
        public readonly static PackedScene Enemy = Load2D("Enemy");
        public readonly static PackedScene Orb = Load2D("Orb");

        // 3D
        public readonly static PackedScene Player3D = Load3D("Player");

        private static PackedScene Load2D(string path) => Load($"2D/{path}");
        private static PackedScene Load3D(string path) => Load($"3D/{path}");
        private static PackedScene LoadPopup(string path) => LoadUI($"Popups/{path}");
        private static PackedScene LoadUI(string path) => Load($"UI/{path}");
        private static PackedScene Load(string path) => ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{path}.tscn");
    }
}

using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        // Game
        public static PackedScene ClientPlayer = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ClientPlayer.tscn");
        public static PackedScene OtherPlayer = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/OtherPlayer.tscn");

        // UI
        public static PackedScene PopupDirectConnect = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupDirectConnect.tscn");
        public static PackedScene PopupError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupError.tscn");
        public static PackedScene PopupMessage = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupMessage.tscn");
        public static PackedScene NotifyError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/NotifyError.tscn");
        public static PackedScene LobbyListing = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyListing.tscn");
        public static PackedScene LobbyPlayerListing = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyPlayerListing.tscn");
        public static PackedScene ModInfo = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/ModInfo.tscn");
    }
}
using Godot;

namespace GodotModules 
{
    public static class Prefabs 
    {
        public static PackedScene PopupDirectConnect = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupDirectConnect.tscn");
        public static PackedScene PopupError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupError.tscn");
        public static PackedScene PopupMessage = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupMessage.tscn");
        public static PackedScene NotifyError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/NotifyError.tscn");
    }
}
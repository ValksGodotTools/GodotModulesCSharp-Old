using Godot;

namespace GodotModules.Netcode 
{
    public class DataPlayer 
    {
        public string Username { get; set; }
        public bool Ready { get; set; }
        public bool Host { get; set; }
        public Direction DirectionHorizontal { get; set; }
        public Direction DirectionVertical { get; set; }
        public Vector2 Position { get; set; }
    }
}
using Godot;

namespace GodotModules.Netcode
{
    public class DataPlayer
    {
        public string Username { get; set; }
        public bool Ready { get; set; }
        public bool Host { get; set; }
        public Direction DirectionHorz { get; set; }
        public Direction DirectionVert { get; set; }
        public Vector2 Position { get; set; }
    }
}
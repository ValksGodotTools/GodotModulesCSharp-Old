using Godot;

namespace GodotModules.Netcode 
{
    public class DataPlayer 
    {
        public string Username { get; set; }
        public bool Ready { get; set; }
        public bool Host { get; set; }
        public bool PressedLeft { get; set; }
        public bool PressedRight { get; set;}
        public bool PressedDown { get; set; }
        public bool PressedUp { get; set; }
        public Vector2 Position { get; set; }
    }
}
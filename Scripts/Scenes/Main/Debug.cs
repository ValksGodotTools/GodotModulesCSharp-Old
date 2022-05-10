using Godot;

namespace GodotModules
{
    public class Debug : Node
    {
        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_test"))
            {
                //GetTree().Root.PrintStrayNodes();

                //var player = GameServer.Players[0];
                //player.Position = Godot.Vector2.Zero;

                GM.SpawnPopupError(new System.Exception("lol beans"));
            }
        }
    }
}
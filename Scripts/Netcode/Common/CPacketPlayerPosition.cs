using Godot;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerPosition : APacketClient 
    {
        public Vector2 Position { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Position.x);
            writer.Write(Position.y);
        }

        public override void Read(PacketReader reader)
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();

            Position = new Vector2(x, y);
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.Log("Client position: " + Position);
        }
    }
}
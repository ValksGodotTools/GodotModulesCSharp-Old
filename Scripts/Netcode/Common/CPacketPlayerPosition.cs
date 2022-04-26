using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerPosition : APacketClient 
    {
        public Vector2 Position { get; set; }

        public override void Write(PacketWriter writer)
        {
            var x = (float)Math.Round(Position.x, 1);
            var y = (float)Math.Round(Position.y, 1);

            writer.Write(x);
            writer.Write(y);
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
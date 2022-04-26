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
            writer.Write(Math.Round(Position.x, 1));
            writer.Write(Math.Round(Position.y, 1));
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
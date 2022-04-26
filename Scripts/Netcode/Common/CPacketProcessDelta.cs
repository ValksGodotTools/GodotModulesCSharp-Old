using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode 
{
    public class CPacketProcessDelta : APacketClient 
    {
        public float Delta { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Delta);
        }

        public override void Read(PacketReader reader)
        {
            Delta = reader.ReadFloat();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.Delta = Delta;
        }
    }
}
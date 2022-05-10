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
            writer.Write((float)Math.Round(Position.x, 1));
            writer.Write((float)Math.Round(Position.y, 1));
        }

        public override void Read(PacketReader reader)
        {
            var pos = new Vector2();
            pos.x = reader.ReadFloat();
            pos.y = reader.ReadFloat();
            Position = pos;
        }

        public override void Handle(ENet.Peer peer)
        {
            NetworkManager.GameServer.Players[(byte)peer.ID].Position = Position;
            ServerSimulation.Enqueue(new ThreadCmd<SimulationOpcode>(SimulationOpcode.PlayerPosition, new SimulationPlayerPosition((byte)peer.ID, Position)));
        }
    }
}
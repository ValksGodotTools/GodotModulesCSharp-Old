using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode
{
    public class CPacketPlayerMovementDirections : APacketClient
    {
        public Direction DirectionHorz { get; set; }
        public Direction DirectionVert { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)DirectionHorz);
            writer.Write((byte)DirectionVert);
        }

        public override void Read(PacketReader reader)
        {
            DirectionHorz = (Direction)reader.ReadByte();
            DirectionVert = (Direction)reader.ReadByte();
        }

        public override void Handle(ENet.Peer peer)
        {
            var player = NetworkManager.GameServer.Players[(byte)peer.ID];
            player.DirectionHorz = DirectionHorz;
            player.DirectionVert = DirectionVert;
        }
    }
}
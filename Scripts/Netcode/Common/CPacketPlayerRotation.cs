using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode
{
    public class CPacketPlayerRotation : APacketClient
    {
        public float Rotation { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((float)Math.Round(Rotation, 1));
        }

        public override void Read(PacketReader reader)
        {
            Rotation = reader.ReadFloat();
        }

        public override void Handle(ENet.Peer peer)
        {
            var player = NetworkManager.GameServer.Players[(byte)peer.ID];
            player.Rotation = Rotation;
        }
    }
}
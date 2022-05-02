using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode
{
    public class CPacketPlayerShoot : APacketClient
    {
        public float PlayerRotation { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((float)PlayerRotation);
        }

        public override void Read(PacketReader reader)
        {
            PlayerRotation = (float)reader.ReadFloat();
        }

        public override void Handle(ENet.Peer peer)
        {
            var player = NetworkManager.GameServer.Players[(byte)peer.ID];
            player.Rotation = PlayerRotation;
        }
    }
}
using System;

namespace GodotModules.Netcode
{
    public class CPacketPlayerRotation : PacketClient
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
            var player = GM.Net.Server.Players[(byte)peer.ID];
            player.Rotation = Rotation;
        }
    }
}
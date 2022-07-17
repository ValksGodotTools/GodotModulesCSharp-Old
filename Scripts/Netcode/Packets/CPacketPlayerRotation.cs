using LiteNetLib;
using GodotModules.Netcode.Server;

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

        public override void Handle(GameServer server, NetPeer peer)
        {
            var player = server.Players[(byte)peer.Id];
            player.Rotation = Rotation;
        }
    }
}
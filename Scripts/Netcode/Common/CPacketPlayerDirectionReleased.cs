using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerDirectionReleased : APacketClient 
    {
        public Direction Direction { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((sbyte)Direction);
        }

        public override void Read(PacketReader reader)
        {
            Direction = (Direction)reader.ReadSByte();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.Log("Released: " + Direction);
        }
    }
}
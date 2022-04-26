using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerDirectionPressed : APacketClient 
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
            GameServer.UpdatePressed(peer.ID, Direction, true);
        }
    }
}
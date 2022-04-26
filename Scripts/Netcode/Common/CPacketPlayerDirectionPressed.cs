using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerDirectionPressed : APacketClient 
    {
        public Direction DirectionHorizontal { get; set; }
        public Direction DirectionVertical { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((sbyte)DirectionHorizontal);
            writer.Write((sbyte)DirectionVertical);
        }

        public override void Read(PacketReader reader)
        {
            DirectionHorizontal = (Direction)reader.ReadSByte();
            DirectionVertical = (Direction)reader.ReadSByte();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.UpdatePressed(peer.ID, DirectionHorizontal, DirectionVertical, true);
        }
    }
}
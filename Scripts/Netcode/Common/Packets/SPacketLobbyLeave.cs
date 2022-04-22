namespace GodotModules.Netcode 
{
    public class SPacketLobbyLeave : IPacket
    {
        public uint Id { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
        }

        public void Read(PacketReader reader)
        {
            Id = reader.ReadUInt16();
        }
    }
}
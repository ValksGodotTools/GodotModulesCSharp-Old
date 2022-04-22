namespace GodotModules.Netcode 
{
    public class SPacketLobbyJoin : IPacket
    {
        public uint Id { get; set; }
        public string Username { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((string)Username);
        }

        public void Read(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            Username = reader.ReadString();
        }
    }
}
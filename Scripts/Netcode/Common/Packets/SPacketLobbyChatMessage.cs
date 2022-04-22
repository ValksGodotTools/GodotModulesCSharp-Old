namespace GodotModules.Netcode 
{
    public class SPacketLobbyChatMessage : IPacketServer
    {
        public uint Id { get; set; }
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((string)Message);
        }

        public void Read(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            Message = reader.ReadString();
        }

        public void Handle()
        {
            SceneLobby.Log(Id, Message);
        }
    }
}
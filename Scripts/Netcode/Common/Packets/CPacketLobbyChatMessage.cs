namespace GodotModules.Netcode 
{
    public class CPacketLobbyChatMessage : IPacket
    {
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Message);
        }

        public void Read(PacketReader reader)
        {
            Message = reader.ReadString();
        }
    }
}
namespace GodotModules.Netcode 
{
    public class CPacketLobbyJoin : IPacket
    {
        public string Username { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Username);
        }

        public void Read(PacketReader reader)
        {
            Username = reader.ReadString();
        }
    }
}
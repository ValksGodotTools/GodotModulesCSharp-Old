using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyJoin : IPacketServer
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

        public void Handle()
        {
            SceneLobby.AddPlayer(Id, Username);

            ENetClient.Log($"Player with username {Username} id: {Id} joined the lobby");
        }
    }
}
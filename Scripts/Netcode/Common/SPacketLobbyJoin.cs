using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyJoin : APacketServerPeerId
    {
        public string Username { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
            writer.Write((string)Username);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
            Username = reader.ReadString();
        }

        public override void Handle()
        {
            SceneLobby.AddPlayer(Id, Username);

            ENetClient.Log($"Player with username {Username} id: {Id} joined the lobby");
        }
    }
}
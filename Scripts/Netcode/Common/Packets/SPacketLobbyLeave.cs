using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyLeave : IPacketServer
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

        public void Handle()
        {
            if (!NetworkManager.GameClient.Players.ContainsKey(Id))
            {
                ENetClient.Log($"Received LobbyLeave packet from server for id {Id}. Tried to remove from Players but does not exist in Players to begin with");
                return;
            }

            SceneLobby.RemovePlayer(Id);

            ENetClient.Log($"Player with id: {Id} left the lobby");
        }
    }
}
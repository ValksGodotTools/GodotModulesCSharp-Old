using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyLeave : PacketServerPeerId
    {
        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
        }

        public override void Handle()
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
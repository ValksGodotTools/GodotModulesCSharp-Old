using GodotModules.Netcode;
using ENet;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyLeave : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new SPacketLobbyLeave();
            data.Read(reader);

            if (!NetworkManager.GameClient.Players.ContainsKey(data.Id))
            {
                Log($"Received LobbyLeave packet from server for id {data.Id}. Tried to remove from Players but does not exist in Players to begin with");
                return;
            }

            SceneLobby.RemovePlayer(data.Id);

            Log($"Player with id: {data.Id} left the lobby");
        }
    }
}
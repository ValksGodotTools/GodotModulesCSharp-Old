using Common.Netcode;
using ENet;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyJoin : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new RPacketLobbyJoin(reader);

            SceneLobby.AddPlayer(data.Id, data.Username);

            Log($"Player with username {data.Username} id: {data.Id} joined the lobby");
        }
    }
}
using Common.Netcode;
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
            var data = new RPacketLobbyLeave(reader);

            UILobby.RemovePlayer(data.Id);

            GD.Print($"Player with id: {data.Id} left the lobby");
        }
    }
}
using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyJoin : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new RPacketLobbyJoin(reader);

            UILobby.AddPlayer(data.Id, data.Username);

            GameManager.ChangeScene("Lobby");
        }
    }
}
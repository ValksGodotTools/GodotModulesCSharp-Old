using Common.Netcode;
using ENet;
using GodotModules.Settings;

namespace GodotModules.Netcode.Client
{
    public class GameClient : ENetClient
    {
        protected override void ProcessGodotCommands(GodotCmd cmd)
        {
            switch (cmd.Opcode)
            {
                case GodotOpcode.AddPlayerToLobbyList:
                    
                    break;
            }
        }

        protected override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
            Outgoing.Enqueue(new ClientPacket((byte)ClientPacketOpcode.LobbyJoin, new WPacketLobbyJoin
            {
                Username = UIOptions.Instance.Options.OnlineUsername
            }));
        }

        protected override void Timeout(Event netEvent)
        {
            GDLog("Client connection timeout");
        }

        protected override void Receive(ServerPacketOpcode opcode, PacketReader reader)
        {
            GDLog($"Received new server packet: {opcode}");

            if (opcode == ServerPacketOpcode.LobbyJoin)
            {
                var data = new RPacketLobbyJoin(reader);
                UILobby.AddPlayer(data.Id, data.Username);

                GetTree().ChangeScene("res://Scenes/Lobby.tscn");
            }

            if (opcode == ServerPacketOpcode.LobbyList)
            {
                var data = new RPacketLobbyList(reader);
                foreach (var player in data.Players)
                    UILobby.AddPlayer(player.Key, player.Value);
            }
        }

        protected override void Disconnect(Event netEvent)
        {
            GDLog("Client disconnected from server");
        }
    }
}
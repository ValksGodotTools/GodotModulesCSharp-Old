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
                    UILobby.AddPlayer((string)cmd.Data);
                    break;
            }
        }

        protected override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
            Outgoing.Enqueue(new ClientPacket((byte)ClientPacketOpcode.LobbyJoin, new WPacketLobbyJoin
            {
                Username = UIOptions.Options.OnlineUsername
            }));
        }

        protected override void Timeout(Event netEvent)
        {
            GDLog("Client connection timeout");
        }

        protected override void Receive(ServerPacketOpcode opcode, PacketReader reader)
        {
            if (opcode == ServerPacketOpcode.LobbyJoin)
            {
                //var data = new RPacketLobbyJoin(reader);
            }
        }

        protected override void Disconnect(Event netEvent)
        {
            GDLog("Client disconnected from server");
        }
    }
}
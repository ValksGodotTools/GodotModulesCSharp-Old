using ENet;
using Common.Netcode;
using Valk.Modules.Settings;

namespace Valk.Modules.Netcode.Client
{
    public class GameClient : ENetClient 
    {
        protected override void ProcessGodotCommands(GodotCmd cmd)
        {
            switch (cmd.Opcode)
            {
                case GodotOpcode.LoadMainMenu:
                    //UIGameMenu.ClientPressedDisconnect = false;
                    //UIMainMenu.LoadMainMenu();
                    break;
            }
        }

        protected override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
            Outgoing.Enqueue(new ClientPacket((byte)ClientPacketOpcode.LobbyJoin, new WPacketLobbyJoin {
                Username = UIOptions.Options.OnlineUsername
            }));
        }

        protected override void Timeout(Event netEvent)
        {
            GDLog("Client connection timeout");
        }

        protected override void Disconnect(Event netEvent)
        {
            GDLog("Client disconnected from server");
        }
    }
}
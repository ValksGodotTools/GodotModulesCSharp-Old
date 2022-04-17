using Common.Netcode;
using ENet;
using GodotModules.Settings;
using System.Collections.Generic;

namespace GodotModules.Netcode.Client
{
    public class GameClient : ENetClient
    {
        public Dictionary<uint, string> Players = new Dictionary<uint, string>();

        protected override void ProcessGodotCommands(GodotCmd cmd)
        {
            
        }

        protected override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
            Outgoing.Enqueue(new ClientPacket((byte)ClientPacketOpcode.LobbyJoin, new WPacketLobbyJoin
            {
                Username = GameManager.Options.OnlineUsername
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
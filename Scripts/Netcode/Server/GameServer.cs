using GodotModules.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        // marking Players as static is okay as long as it is created a new in _Ready() (same goes for any other statics)
        public static Dictionary<uint, string> Players { get; set; } // the players in a lobby

        public GameServer()
        {
            Players = new Dictionary<uint, string>();
        }

        protected override void Connect(Event netEvent)
        {
        }

        protected override void Disconnect(Event netEvent)
        {
            HandlePlayerLeave(netEvent.Peer.ID);
        }

        protected override void Timeout(Event netEvent)
        {
            HandlePlayerLeave(netEvent.Peer.ID);
        }

        protected override void Stopped()
        {
        }

        private void HandlePlayerLeave(uint id)
        {
            // tell other players that this player left lobby
            Send(ServerPacketOpcode.LobbyLeave, new WPacketLobbyLeave { Id = id}, GetOtherPeers(id));

            if (Players.ContainsKey(id))
                Players.Remove(id);
        }

        public static Dictionary<uint, string> GetOtherPlayers(uint id)
        {
            var otherPlayers = new Dictionary<uint, string>(Players);
            otherPlayers.Remove(id);
            return otherPlayers;
        }

        public static Peer[] GetAllPlayerPeers() => Players.Keys.Select(x => Peers[x]).ToArray();
    }
}
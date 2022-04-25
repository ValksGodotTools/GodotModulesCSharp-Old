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
        public static Dictionary<uint, DataPlayer> Players { get; set; } // the players in a lobby

        public GameServer()
        {
            Players = new Dictionary<uint, DataPlayer>();
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
            Send(ServerPacketOpcode.LobbyLeave, new SPacketLobbyLeave { Id = id}, GetOtherPeers(id));

            if (Players.ContainsKey(id))
                Players.Remove(id);
        }

        public static Dictionary<uint, DataPlayer> GetOtherPlayers(uint id)
        {
            var otherPlayers = new Dictionary<uint, DataPlayer>(Players);
            otherPlayers.Remove(id);
            return otherPlayers;
        }

        public static Peer[] GetOtherPlayerPeers(uint id) => Players.Keys.Where(x => x != id).Select(x => Peers[x]).ToArray();
        public static Peer[] GetAllPlayerPeers() => Players.Keys.Select(x => Peers[x]).ToArray();

        public static void SendToAllPlayers(ServerPacketOpcode opcode, IPacket data = null)
        {
            var allPlayers = GetAllPlayerPeers();

            if (data == null)
                Send(opcode, allPlayers);
            else
                Send(opcode, data, allPlayers);
        }

        public static void SendToOtherPeers(uint id, ServerPacketOpcode opcode, IPacket data = null)
        {
            var otherPeers = GetOtherPeers(id);
            if (otherPeers.Length == 0)
                return;

            if (data == null)
                Send(opcode, otherPeers);
            else
                Send(opcode, data, otherPeers);
        }

        public static void SendToOtherPlayers(uint id, ServerPacketOpcode opcode, IPacket data = null)
        {
            var otherPlayers = GetOtherPlayerPeers(id);
            if (otherPlayers.Length == 0)
                return;

            if (data == null)
                Send(opcode, otherPlayers);
            else
                Send(opcode, data, otherPlayers);
        }
    }
}
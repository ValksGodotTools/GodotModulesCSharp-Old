using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        public Dictionary<uint, string> Players { get; set; } // the players in a lobby

        public override void _Ready()
        {
            base._Ready();
            Players = new Dictionary<uint, string>();
        }

        protected override void Connect(Event netEvent)
        {
        }

        protected override void Disconnect(Event netEvent)
        {
            if (Players.ContainsKey(netEvent.Peer.ID))
                Players.Remove(netEvent.Peer.ID);
        }

        protected override void Timeout(Event netEvent)
        {
            if (Players.ContainsKey(netEvent.Peer.ID))
                Players.Remove(netEvent.Peer.ID);
        }

        protected override void Stopped()
        {
        }
    }
}
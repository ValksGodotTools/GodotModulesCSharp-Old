using System.Collections.Generic;
using ENet;
using Common.Game;

namespace Valk.Modules.Netcode.Server 
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        private static Dictionary<uint, Player> Players { get; set; }

        public override void _Ready()
        {
            base._Ready();
            Players = new Dictionary<uint, Player>();
        }

        protected override void Connect(Event netEvent)
        {
            
        }

        protected override void Disconnect(Event netEvent)
        {
            
        }

        protected override void Timeout(Event netEvent)
        {
            
        }
    }
}
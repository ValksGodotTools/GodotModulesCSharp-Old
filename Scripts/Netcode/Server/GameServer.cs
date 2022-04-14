using System.IO;
using System.Collections.Generic;
using ENet;
using Common.Game;
using Valk.Modules;

namespace Valk.Modules.Netcode.Server 
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        private static Dictionary<uint, Player> Players { get; set; }
        private static string PathServer => Path.Combine(FileManager.GetGameDataPath(), "Server/Players");

        public override void _Ready()
        {
            base._Ready();
            Players = new Dictionary<uint, Player>();

            Directory.CreateDirectory(PathServer);

            GetPlayerConfigs();
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

        private static void GetPlayerConfigs()
        {
            foreach (var file in Directory.GetFiles(PathServer))
                GDLog(file);

            // TODO: store all player configs to <Player>'s in Dict<uint, Player> where uint is ENet.Peer.Id
        }
    }
}
using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.IO;

namespace GodotModules.Netcode.Server
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        public static Dictionary<uint, string> Players { get; set; }

        private static string PathServer => Path.Combine(FileManager.GetGameDataPath(), "Server");
        private static string PathPlayers => Path.Combine(PathServer, "Players");

        public override void _Ready()
        {
            base._Ready();
            Players = new Dictionary<uint, string>();
            Directory.CreateDirectory(PathServer);
        }

        protected override void Connect(Event netEvent)
        {
        }

        protected override void Receive(Event netEvent, ClientPacketOpcode opcode, PacketReader reader)
        {
            if (opcode == ClientPacketOpcode.LobbyJoin)
            {
                var data = new RPacketLobbyJoin(reader);

                GameServer.Players.Add(netEvent.Peer.ID, data.Username);

                GodotCmds.Enqueue(new GodotCmd
                {
                    Opcode = GodotOpcode.AddPlayerToLobbyList,
                    Data = data.Username
                });
            }
        }

        protected override void Disconnect(Event netEvent)
        {
            //GameServer.Players.Remove(netEvent.Peer.ID);
        }

        protected override void Timeout(Event netEvent)
        {
            //GameServer.Players.Remove(netEvent.Peer.ID);
        }

        protected override void Stopped()
        {
        }
    }
}
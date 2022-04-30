using ENet;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        public static Dictionary<uint, DataPlayer> Players { get; set; }
        public static Timer EmitClientPositions { get; set; }
        public static float Delta { get; set; }

        public GameServer()
        {
            Players = new();

            EmitClientPositions = new(CommandDebug.SendReceiveDataInterval);
            EmitClientPositions.Elapsed += EmitClientPositionsCallback;
            EmitClientPositions.AutoReset = true;
        }

        public void EmitClientPositionsCallback(System.Object source, ElapsedEventArgs args)
        {
            Players.ForEach(pair =>
                Send(ServerPacketOpcode.PlayerPositions, new SPacketPlayerPositions
                {
                    PlayerPositions = Players.Where(x => x.Key != pair.Key).ToDictionary(x => x.Key, x => x.Value.Position)
                }, Peers[pair.Key])
            );
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
            Send(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyLeave,
                Id = id
            }, GetOtherPeers(id));

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

        public static void SendToAllPlayers(ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            var allPlayers = GetAllPlayerPeers();

            if (data == null)
                Send(opcode, flags, allPlayers);
            else
                Send(opcode, data, flags, allPlayers);
        }

        public static void SendToOtherPeers(uint id, ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            var otherPeers = GetOtherPeers(id);
            if (otherPeers.Length == 0)
                return;

            if (data == null)
                Send(opcode, flags, otherPeers);
            else
                Send(opcode, data, flags, otherPeers);
        }

        public static void SendToOtherPlayers(uint id, ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            var otherPlayers = GetOtherPlayerPeers(id);
            if (otherPlayers.Length == 0)
                return;

            if (data == null)
                Send(opcode, flags, otherPlayers);
            else
                Send(opcode, data, flags, otherPlayers);
        }
    }
}
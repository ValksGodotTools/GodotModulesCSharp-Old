using Timer = System.Timers.Timer;

using Godot;
using GodotModules.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        public static Dictionary<uint, DataPlayer> Players { get; set; }
        public static Dictionary<uint, Vector2> LastSentPlayerPositions { get; set; }
        public static Timer TimerGameLoop { get; set; }
        public static Timer TimerNotifyClients { get; set; }
        public static float Delta { get; set; }

        public GameServer()
        {
            Players = new Dictionary<uint, DataPlayer>();
            LastSentPlayerPositions = new Dictionary<uint, Vector2>();
            TimerGameLoop = new Timer(16.67);
            TimerGameLoop.Elapsed += TimerGameLoopCallback;
            TimerGameLoop.AutoReset = true;

            TimerNotifyClients = new Timer(200);
            TimerNotifyClients.Elapsed += TimerNotifyClientsCallback;
            TimerNotifyClients.AutoReset = true;
        }

        public void TimerNotifyClientsCallback(System.Object source, ElapsedEventArgs args)
        {
            var playerPositions = Players.ToDictionary(x => x.Key, x => x.Value.Position);

            SendToAllPlayers(ServerPacketOpcode.PlayerPositions, new SPacketPlayerPositions {
                PlayerPositions = playerPositions
            }, PacketFlags.Reliable);

            LastSentPlayerPositions = playerPositions;

            System.Console.WriteLine("SERVER: " + playerPositions[0]);
        }

        public void TimerGameLoopCallback(System.Object source, ElapsedEventArgs args) 
        {
            foreach (var pair in Players)
            {
                var player = pair.Value;

                var dir = new Vector2();

                if (player.DirectionHorizontal == Direction.West)
                    dir.x -= 1;
                if (player.DirectionHorizontal == Direction.East)
                    dir.x += 1;
                if (player.DirectionVertical == Direction.North)
                    dir.y -= 1;
                if (player.DirectionVertical == Direction.South)
                    dir.y += 1;

                var delta = 0.016667f;

                player.Position += dir * 250 * delta;
            }
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

        public static void UpdatePressed(uint id, Direction directionHorizontal, Direction directionVertical, bool pressed)
        {
            var player = Players[id];
            player.DirectionHorizontal = directionHorizontal;
            player.DirectionVertical = directionVertical;
        }

        public static void CheckPosition(uint id, Vector2 position)
        {
            var player = Players[id];
            Log("Distance: " + position.DistanceSquaredTo(player.Position));
        }
    }
}
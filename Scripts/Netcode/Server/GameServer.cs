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
        public static Timer TimerGameLoop { get; set; }
        public static Timer TimerNotifyClients { get; set; }
        public static float Delta { get; set; }

        public GameServer()
        {
            Players = new Dictionary<uint, DataPlayer>();
            TimerGameLoop = new Timer(16.67);
            TimerGameLoop.Elapsed += TimerGameLoopCallback;
            TimerGameLoop.AutoReset = true;

            TimerNotifyClients = new Timer(1000);
            TimerNotifyClients.Elapsed += TimerNotifyClientsCallback;
            TimerNotifyClients.AutoReset = true;
        }

        public void TimerNotifyClientsCallback(System.Object source, ElapsedEventArgs args)
        {
            SendToAllPlayers(ServerPacketOpcode.PlayerPositions, new SPacketPlayerPositions {
                PlayerPositions = Players.ToDictionary(x => x.Key, x => x.Value.Position)
            });
        }

        public void TimerGameLoopCallback(System.Object source, ElapsedEventArgs args) 
        {
            foreach (var pair in Players)
            {
                var player = pair.Value;

                var dir = new Vector2();

                if (player.PressedLeft)
                    dir.x = -1;
                if (player.PressedRight)
                    dir.x = 1;
                if (player.PressedUp)
                    dir.y = -1;
                if (player.PressedDown)
                    dir.y = 1;

                player.Position += dir * 250 * Delta;
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

        public static void SendToAllPlayers(ServerPacketOpcode opcode, APacket data = null)
        {
            var allPlayers = GetAllPlayerPeers();

            if (data == null)
                Send(opcode, allPlayers);
            else
                Send(opcode, data, allPlayers);
        }

        public static void SendToOtherPeers(uint id, ServerPacketOpcode opcode, APacket data = null)
        {
            var otherPeers = GetOtherPeers(id);
            if (otherPeers.Length == 0)
                return;

            if (data == null)
                Send(opcode, otherPeers);
            else
                Send(opcode, data, otherPeers);
        }

        public static void SendToOtherPlayers(uint id, ServerPacketOpcode opcode, APacket data = null)
        {
            var otherPlayers = GetOtherPlayerPeers(id);
            if (otherPlayers.Length == 0)
                return;

            if (data == null)
                Send(opcode, otherPlayers);
            else
                Send(opcode, data, otherPlayers);
        }

        public static void UpdatePressed(uint id, Direction dir, bool pressed)
        {
            var player = Players[id];
            switch (dir) 
            {
                case Direction.West:
                    player.PressedLeft = pressed;
                    break;
                case Direction.East:
                    player.PressedRight = pressed;
                    break;
                case Direction.North:
                    player.PressedUp = pressed;
                    break;
                case Direction.South:
                    player.PressedDown = pressed;
                    break;
            }
        }

        public static void CheckPosition(uint id, Vector2 position)
        {
            var player = Players[id];
            Log("Distance: " + position.DistanceSquaredTo(player.Position));
        }
    }
}
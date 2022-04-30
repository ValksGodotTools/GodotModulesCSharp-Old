using ENet;

using System.Timers;
using Timer = System.Timers.Timer;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        public static Dictionary<uint, DataPlayer> Players { get; set; }
        private static STimer EmitClientPositions { get; set; }
        private static STimer ServerSimulation { get; set; }

        public GameServer()
        {
            Players = new();

            EmitClientPositions = new(CommandDebug.SendReceiveDataInterval, () =>
            {
                SendToAllPlayers(ServerPacketOpcode.PlayerPositions, new SPacketPlayerPositions
                {
                    PlayerPositions = Players.ToDictionary(x => x.Key, x => x.Value.Position)
                });
            }, false);

            const int oneSecondInMs = 1000;
            const int fps = 60;
            const float delta = 1f / fps;

            ServerSimulation = new((double)oneSecondInMs / fps, () => {
                Players.ForEach(x => {
                    var player = x.Value;
                    var directionVert = player.DirectionVert;
                    var directionHorz = player.DirectionHorz;

                    var dir = new Godot.Vector2();

                    if (directionVert == Direction.Up)
                        dir.y -= 1;
                    if (directionVert == Direction.Down)
                        dir.y += 1;
                    if (directionHorz == Direction.Left)
                        dir.x -= 1;
                    if (directionHorz == Direction.Right)
                        dir.x += 1;

                    const int speed = 250;

                    player.Position += dir * speed * delta;
                });
            }, false);
        }

        public static void StartGame()
        {
            EmitClientPositions.Start();
            ServerSimulation.Start();
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

        public override void Dispose()
        {
            EmitClientPositions.Dispose();
            ServerSimulation.Dispose();
        }
    }
}
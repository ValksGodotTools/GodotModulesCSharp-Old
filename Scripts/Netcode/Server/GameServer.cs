using ENet;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        public DataLobby Lobby { get; set; }
        public Dictionary<byte, DataPlayer> Players { get; set; }
        public Dictionary<ushort, DataBullet> Bullets { get; set; }
        public STimer EmitClientTransforms { get; set; }
        private STimer ServerSimulation { get; set; }
        public bool DisallowJoiningLobby { get; set; }

        public GameServer()
        {
            Players = new();
            Lobby = new();

            EmitClientTransforms = new(ServerIntervals.PlayerTransforms, () =>
            {
                SendToAllPlayers(ServerPacketOpcode.PlayerTransforms, new SPacketPlayerTransforms
                {
                    PlayerTransforms = Players.ToDictionary(x => x.Key, x => new DataEntityTransform
                    {
                        Position = x.Value.Position,
                        Rotation = x.Value.Rotation
                    })
                });
            }, false);

            const int oneSecondInMs = 1000;
            const int fps = 60;
            const float delta = 1f / fps;

            ServerSimulation = new((double)oneSecondInMs / fps, () =>
            {
                Players.ForEach(x =>
                {
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

        protected override void ServerCmds()
        {
            while (ENetCmds.TryDequeue(out ENetCmd cmd))
            {
                var opcode = cmd.Opcode;

                // Host client wants to stop the server
                switch (opcode)
                {
                    case ENetOpcode.StartGame:
                        EmitClientTransforms.Start();
                        ServerSimulation.Start();
                        break;

                    case ENetOpcode.StopServer:
                        if (CTS.IsCancellationRequested)
                        {
                            Log("Server has been stopped already");
                            return;
                        }

                        CTS.Cancel();

                        KickAll(DisconnectOpcode.Stopping);
                        break;

                    case ENetOpcode.RestartServer:
                        if (CTS.IsCancellationRequested)
                        {
                            Log("Server has been stopped already");
                            return;
                        }

                        KickAll(DisconnectOpcode.Restarting);

                        QueueRestart = true;
                        break;

                    case ENetOpcode.ClientWantsToExitApp:
                        CTS.Cancel();
                        KickAll(DisconnectOpcode.Stopping);
                        break;

                    case ENetOpcode.Dispose:
                        CTS.Dispose();
                        EmitClientTransforms.Stop();
                        EmitClientTransforms.Dispose();
                        ServerSimulation.Stop();
                        ServerSimulation.Dispose();
                        break;
                }
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
            if (!Players.ContainsKey((byte)id)) // because dummy clients can connect for ping test and are not actual players
                return;

            // tell other players that this player left lobby
            Send(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyLeave,
                Id = (byte)id
            }, GetOtherPeers(id));

            Players.Remove((byte)id);
        }

        public Dictionary<byte, DataPlayer> GetOtherPlayers(byte id)
        {
            var otherPlayers = new Dictionary<byte, DataPlayer>(Players);
            otherPlayers.Remove(id);
            return otherPlayers;
        }

        public Peer[] GetOtherPlayerPeers(uint id) => Players.Keys.Where(x => x != id).Select(x => Peers[x]).ToArray();

        public Peer[] GetAllPlayerPeers() => Players.Keys.Select(x => Peers[x]).ToArray();

        public void SendToAllPlayers(ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            var allPlayers = GetAllPlayerPeers();

            if (data == null)
                Send(opcode, flags, allPlayers);
            else
                Send(opcode, data, flags, allPlayers);
        }

        public void SendToOtherPeers(uint id, ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            var otherPeers = GetOtherPeers(id);
            if (otherPeers.Length == 0)
                return;

            if (data == null)
                Send(opcode, flags, otherPeers);
            else
                Send(opcode, data, flags, otherPeers);
        }

        public void SendToOtherPlayers(uint id, ServerPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
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
            base.Dispose();
            ENetCmds.Enqueue(new ENetCmd(ENetOpcode.Dispose));
        }
    }
}
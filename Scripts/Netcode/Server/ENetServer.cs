using ENet;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace GodotModules.Netcode.Server
{
    public abstract class ENetServer
    {
        public bool HasSomeoneConnected { get => Interlocked.Read(ref SomeoneConnected) == 1; }
        public bool IsRunning { get => Interlocked.Read(ref Running) == 1; }
        public Dictionary<uint, Peer> Peers { get; set; }
        public ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        public ushort MaxPlayers { get; set; }

        protected CancellationTokenSource CTS { get; set; }
        protected bool QueueRestart { get; set; }
        protected long SomeoneConnected = 0;
        protected long Running = 0;

        private static readonly Dictionary<ClientPacketOpcode, APacketClient> HandlePacket = ReflectionUtils.LoadInstances<ClientPacketOpcode, APacketClient>("CPacket");
        private ConcurrentQueue<ServerPacket> Outgoing { get; set; }

        public ENetServer()
        {
            ENetCmds = new();
            Outgoing = new();
            Peers = new();
        }

        public async Task Start(ushort port, int maxClients)
        {
            if (IsRunning)
            {
                Log("Server is running already");
                return;
            }

            CTS = new CancellationTokenSource();

            try
            {
                await Task.Run(() => ENetThreadWorker(port, maxClients), CTS.Token);
            }
            catch (Exception e)
            {
                Logger.LogErr(e, "Server");
            }
        }

        public void KickAll(DisconnectOpcode opcode)
        {
            Peers.Values.ForEach(peer => peer.DisconnectNow((uint)opcode));
            Peers.Clear();
        }

        public void Kick(uint id, DisconnectOpcode opcode)
        {
            Peers[id].DisconnectNow((uint)opcode);
            Peers.Remove(id);
        }

        public void Stop() => ENetCmds.Enqueue(new ENetCmd(ENetOpcode.StopServer));

        public void Restart() => ENetCmds.Enqueue(new ENetCmd(ENetOpcode.RestartServer));

        public void Send(ServerPacketOpcode opcode, params Peer[] peers) => Send(opcode, null, PacketFlags.Reliable, peers);

        public void Send(ServerPacketOpcode opcode, APacket data, params Peer[] peers) => Send(opcode, data, PacketFlags.Reliable, peers);

        public void Send(ServerPacketOpcode opcode, PacketFlags flags = PacketFlags.Reliable, params Peer[] peers) => Send(opcode, null, flags, peers);

        public void Send(ServerPacketOpcode opcode, APacket data, PacketFlags flags = PacketFlags.Reliable, params Peer[] peers) => Outgoing.Enqueue(new ServerPacket((byte)opcode, flags, data, peers));

        public void Log(object obj) => Logger.Log($"[Server]: {obj}", ConsoleColor.Cyan);

        protected Peer[] GetOtherPeers(uint id)
        {
            var otherPeers = new Dictionary<uint, Peer>(Peers);
            otherPeers.Remove(id);
            return otherPeers.Values.ToArray();
        }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Connect(Event netEvent)
        { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Disconnect(Event netEvent)
        { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Timeout(Event netEvent)
        { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        protected virtual void Stopped()
        { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        protected virtual void ServerCmds()
        { }

        private async Task ENetThreadWorker(ushort port, int maxClients)
        {
            Thread.CurrentThread.Name = "Server";
            if (SceneLobby.CurrentLobby != null)
                if (SceneLobby.CurrentLobby.Public && WebClient.ConnectionAlive)
                    WebClient.TimerPingMasterServer.Start();

            MaxPlayers = (ushort)maxClients;

            Library.Initialize();

            using var server = new Host();
            Address address = new Address();
            address.Port = port;

            try
            {
                server.Create(address, maxClients);
            }
            catch (Exception e)
            {
                var message = $"A server is running on port {port} already! {e.Message}";
                Log(message);
                NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.PopupMessage, message));
                NetworkManager.GameClient.Stop();
                Stop();
                return;
            }

            Log($"Server listening on port {port}");
            Running = 1;

            while (!CTS.IsCancellationRequested)
            {
                bool polled = false;

                // ENet Cmds
                ServerCmds();

                // Outgoing
                while (Outgoing.TryDequeue(out ServerPacket packet))
                    packet.Peers.ForEach(peer => Send(packet, peer));

                while (!polled)
                {
                    if (server.CheckEvents(out Event netEvent) <= 0)
                    {
                        if (server.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    var peer = netEvent.Peer;
                    var eventType = netEvent.Type;

                    if (eventType == EventType.Receive)
                    {
                        // Receive
                        var packet = netEvent.Packet;
                        if (packet.Length > GamePacket.MaxSize)
                        {
                            Log($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
                            packet.Dispose();
                            continue;
                        }

                        var packetReader = new PacketReader(packet);
                        var opcode = (ClientPacketOpcode)packetReader.ReadByte();

                        //Log($"Received packet: {opcode}");

                        if (!HandlePacket.ContainsKey(opcode))
                        {
                            Logger.LogWarning($"[Server]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException e)
                        {
                            Logger.LogWarning($"[Server]: Received malformed opcode: {opcode} {e.Message} (Ignoring)");
                            break;
                        }
                        handlePacket.Handle(netEvent.Peer);

                        packetReader.Dispose();
                    }
                    else if (eventType == EventType.Connect)
                    {
                        Log($"Client connected with id: {netEvent.Peer.ID}");

                        // Connect
                        SomeoneConnected = 1;
                        Peers[netEvent.Peer.ID] = netEvent.Peer;
                        Connect(netEvent);
                    }
                    else if (eventType == EventType.Disconnect)
                    {
                        Log($"Client disconnected with id: {netEvent.Peer.ID}");

                        // Disconnect
                        Peers.Remove(netEvent.Peer.ID);
                        Disconnect(netEvent);
                    }
                    else if (eventType == EventType.Timeout)
                    {
                        // Timeout
                        Peers.Remove(netEvent.Peer.ID);
                        Timeout(netEvent);
                    }
                }
            }

            server.Flush();

            Log("Server stopped");

            while (ConcurrentQueuesWorking())
                await Task.Delay(100);

            Running = 0;
            Stopped();

            if (QueueRestart)
            {
                QueueRestart = false;
                //Start();
                NetworkManager.StartServer(port, maxClients);
            }
        }

        private void Send(ServerPacket gamePacket, Peer peer)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, gamePacket.PacketFlags);
            byte channelID = 0;
            peer.Send(channelID, ref packet);
        }

        private bool ConcurrentQueuesWorking() => ENetCmds.Count != 0 || Outgoing.Count != 0;
    }
}
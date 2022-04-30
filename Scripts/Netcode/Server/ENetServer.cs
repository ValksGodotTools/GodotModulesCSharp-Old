using ENet;
using System;
using System.Collections.Concurrent;

using System.Threading;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace GodotModules.Netcode.Server
{
    public abstract class ENetServer : IDisposable
    {
        private static CancellationTokenSource CancelTokenSource { get; set; }
        public static ConsoleColor LogsColor = ConsoleColor.Cyan;
        public static bool SomeoneConnected { get; set; }
        public static bool Running { get; set; }
        private static ConcurrentQueue<ServerPacket> Outgoing { get; set; }
        public static Dictionary<uint, Peer> Peers { get; set; }
        private static readonly Dictionary<ClientPacketOpcode, APacketClient> HandlePacket = ReflectionUtils.LoadInstances<ClientPacketOpcode, APacketClient>("CPacket");
        public ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        private bool QueueRestart { get; set; }

        public ENetServer()
        {
            Running = false;
            SomeoneConnected = false;
            ENetCmds = new();
            Outgoing = new();
            Peers = new();
        }

        /// <summary>
        /// The server thread worker
        /// </summary>
        /// <param name="port"></param>
        /// <param name="maxClients"></param>
        public async Task ENetThreadWorker(ushort port, int maxClients)
        {
            Thread.CurrentThread.Name = "Server";
            if (SceneLobby.CurrentLobby.Public)
                NetworkManager.WebClient.TimerPingMasterServer.Start();

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
                await GodotModules.Netcode.Client.ENetClient.Stop();
                await Stop();
                return;
            }

            Log($"Server listening on port {port}");
            Running = true;

            while (!CancelTokenSource.IsCancellationRequested)
            {
                bool polled = false;

                // ENet Cmds
                while (ENetCmds.TryDequeue(out ENetCmd cmd))
                {
                    var opcode = cmd.Opcode;

                    // Host client wants to stop the server
                    switch (opcode)
                    {
                        case ENetOpcode.ClientWantsToExitApp:
                            CancelTokenSource.Cancel();
                            KickAll(DisconnectOpcode.Stopping);
                            break;
                    }
                }

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
                            Utils.LogErr($"[Server]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException)
                        {
                            Utils.LogErr($"[Server]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }
                        handlePacket.Handle(netEvent.Peer);

                        packetReader.Dispose();
                    }
                    else if (eventType == EventType.Connect)
                    {
                        // Connect
                        SomeoneConnected = true;
                        Peers.Add(netEvent.Peer.ID, netEvent.Peer);
                        Connect(netEvent);
                    }
                    else if (eventType == EventType.Disconnect)
                    {
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

            Running = false;
            Stopped();

            if (QueueRestart)
            {
                QueueRestart = false;
                //Start();
                NetworkManager.StartServer(port, maxClients);
            }
        }

        private bool ConcurrentQueuesWorking() => NetworkManager.GodotCmds.Count != 0 || ENetCmds.Count != 0 || Outgoing.Count != 0;

        /// <summary>
        /// Start the server, can be called from the Godot thread
        /// </summary>
        public async Task Start(ushort port, int maxClients)
        {
            if (Running)
            {
                Log("Server is running already");
                return;
            }

            CancelTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Run(() => ENetThreadWorker(port, maxClients), CancelTokenSource.Token);
            }
            catch (Exception e)
            {
                NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.Error, e));
            }
        }

        /// <summary>
        /// Stop the server, can be called from the Godot thread
        /// </summary>
        public static async Task Stop()
        {
            if (CancelTokenSource.IsCancellationRequested)
            {
                Log("Server has been stopped already");
                return;
            }

            CancelTokenSource.Cancel();

            KickAll(DisconnectOpcode.Stopping);

            while (!CancelTokenSource.IsCancellationRequested)
                await Task.Delay(100);
        }

        /// <summary>
        /// Restart the server, can be called from the Godot thread
        /// </summary>
        public void Restart()
        {
            if (CancelTokenSource.IsCancellationRequested)
            {
                Log("Server has been stopped already");
                return;
            }

            KickAll(DisconnectOpcode.Restarting);

            QueueRestart = true;
        }

        public static Peer[] GetOtherPeers(uint id)
        {
            var otherPeers = new Dictionary<uint, Peer>(Peers);
            otherPeers.Remove(id);
            return otherPeers.Values.ToArray();
        }

        public static void Send(ServerPacketOpcode opcode, params Peer[] peers) => Send(opcode, null, PacketFlags.Reliable, peers);

        public static void Send(ServerPacketOpcode opcode, APacket data, params Peer[] peers) => Send(opcode, data, PacketFlags.Reliable, peers);

        public static void Send(ServerPacketOpcode opcode, PacketFlags flags = PacketFlags.Reliable, params Peer[] peers) => Send(opcode, null, flags, peers);

        public static void Send(ServerPacketOpcode opcode, APacket data, PacketFlags flags = PacketFlags.Reliable, params Peer[] peers) => Outgoing.Enqueue(new ServerPacket((byte)opcode, flags, data, peers));

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void Log(object obj) => NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessageServer, obj));

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
        /// Send a packet to a client. This method is not meant to be used directly, see Outgoing ConcurrentQueue
        /// </summary>
        /// <param name="gamePacket">The packet to send</param>
        /// <param name="peer">The peer to send this packet to</param>
        private void Send(ServerPacket gamePacket, Peer peer)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, gamePacket.PacketFlags);
            byte channelID = 0;
            peer.Send(channelID, ref packet);
        }

        /// <summary>
        /// Kick all clients from the server
        /// </summary>
        /// <param name="opcode">Tells the client why they were kicked</param>
        private static void KickAll(DisconnectOpcode opcode)
        {
            Peers.Values.ForEach(peer => peer.DisconnectNow((uint)opcode));
            Peers.Clear();
        }

        /// <summary>
        /// Kick a specified client from the server
        /// </summary>
        /// <param name="id"></param>
        /// <param name="opcode">Tells the client why they were kicked</param>
        private static void Kick(uint id, DisconnectOpcode opcode)
        {
            Peers[id].DisconnectNow((uint)opcode);
            Peers.Remove(id);
        }

        public virtual void Dispose()
        {
            CancelTokenSource.Dispose();
        }
    }
}
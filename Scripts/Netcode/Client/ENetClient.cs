using ENet;
using System;
using System.Collections.Concurrent;

using System.Threading;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace GodotModules.Netcode.Client
{
    public class ENetClient : IDisposable
    {
        public static readonly Dictionary<ServerPacketOpcode, APacketServer> HandlePacket = ReflectionUtils.LoadInstances<ServerPacketOpcode, APacketServer>("SPacket");

        // values grabbed from the server
        // =========================================================
        public uint PeerId { get; set; } // this clients peer id (grabbed from server at some point)
        public bool IsHost { get; set; }
        // =========================================================

        public DateTime PingSent { get; set; }
        public int PingMs { get; set;}
        public bool WasPingReceived { get; set; }
        protected CancellationTokenSource CancelTokenSource { get; set; }
        public ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        private int OutgoingId { get; set; }
        private ConcurrentDictionary<int, ClientPacket> Outgoing { get; set; }
        public DisconnectOpcode DisconnectOpcode { get; set; }
        protected long Connected = 0;
        public bool IsConnected { get => Interlocked.Read(ref Connected) == 1; } // thread safe

        public bool Running { get; set; }
        protected bool ENetThreadRunning;

        public ENetClient()
        {
            Running = false;
            Connected = 0;
            OutgoingId = 0;
            Outgoing = new();
            ENetCmds = new();
            DisconnectOpcode = DisconnectOpcode.Disconnected;
        }

        /// <summary>
        /// The client thread worker
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private async Task ENetThreadWorker(string ip, ushort port)
        {
            Thread.CurrentThread.Name = "Client";
            Library.Initialize();

            using var client = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            client.Create();

            //GDLog("Attempting to connect to the game server...");
            var peer = client.Connect(address);

            uint pingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
            uint timeout = 5000; // Will be ignored if maximum timeout is exceeded
            uint timeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
            uint timeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

            peer.PingInterval(pingInterval);
            peer.Timeout(timeout, timeoutMinimum, timeoutMaximum);

            Running = true;

            while (!CancelTokenSource.IsCancellationRequested)
            {
                var polled = false;

                // ENet Cmds from Godot Thread
                while (ENetCmds.TryDequeue(out ENetCmd cmd))
                {
                    switch (cmd.Opcode)
                    {
                        case ENetOpcode.ClientWantsToExitApp:
                        case ENetOpcode.ClientWantsToDisconnect:
                            peer.Disconnect(0);
                            break;
                    }
                }

                // Outgoing
                while (Outgoing.TryRemove(OutgoingId, out ClientPacket clientPacket))
                {
                    OutgoingId--;
                    byte channelID = 0; // The channel all networking traffic will be going through
                    var packet = default(Packet);
                    packet.Create(clientPacket.Data, clientPacket.PacketFlags);
                    //Log("Sent packet: " + (ClientPacketOpcode)clientPacket.Opcode);
                    peer.Send(channelID, ref packet);
                    PingSent = DateTime.Now;
                }

                while (!polled)
                {
                    if (client.CheckEvents(out Event netEvent) <= 0)
                    {
                        if (client.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.Connect:
                            Connected = 1;
                            Connect(netEvent);
                            break;

                        case EventType.Receive:
                            // Receive
                            var packet = netEvent.Packet;
                            if (packet.Length > GamePacket.MaxSize)
                            {
                                Log($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
                                packet.Dispose();
                                continue;
                            }

                            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ENetPacket, new PacketHandleData {
                                Reader = new PacketReader(packet),
                                Client = this
                            }));
                            break;

                        case EventType.Timeout:
                            Timeout(netEvent);
                            break;

                        case EventType.Disconnect:
                            Disconnect(netEvent);
                            break;
                    }
                }

                client.Flush();
            }

            Library.Deinitialize();
            ENetThreadRunning = false;

            Log($"Client stopped");

            while (ConcurrentQueuesWorking())
                await Task.Delay(100);

            Running = false;
        }

        public async Task Send(ClientPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            OutgoingId++;
            var success = Outgoing.TryAdd(OutgoingId, new ClientPacket((byte)opcode, flags, data));

            if (!success)
                Log($"Failed to add {opcode} to Outgoing queue because of duplicate key"); // this should never go off, however it's better to be safe then not safe at all

            while (Outgoing.ContainsKey(OutgoingId))
                await Task.Delay(100);
        }

        private bool ConcurrentQueuesWorking() => NetworkManager.GodotCmds.Count != 0 || ENetCmds.Count != 0 || Outgoing.Count != 0;

        /// <summary>
        /// Attempt to connect to the server, can be called from the Godot thread
        /// Because Task is not returned we surround all code with try catch to catch exceptions
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async void Start(string ip, ushort port)
        {
            try
            {
                if (ENetThreadRunning)
                {
                    SceneGameServers.ConnectingToLobby = false;
                    Log($"Client is running already");
                    return;
                }

                ENetThreadRunning = true;
                CancelTokenSource = new CancellationTokenSource();

                await Task.Run(() => ENetThreadWorker(ip, port), CancelTokenSource.Token);
            }
            catch (Exception e)
            {
                NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.Error, e));
            }
        }

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public void Stop()
        {
            //CancelTokenSource.Cancel();
            ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToDisconnect));
        }

        public void CancelTask()
        {
            CancelTokenSource.Cancel();
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// Checks thread name, if its Client send request to log on Godot thread otherwise log on Godot thread directly
        /// </summary>
        /// <param name="obj">The object to log</param>
        public void Log(object obj) => NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessageClient, obj));

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

        public void Dispose()
        {
            CancelTokenSource.Dispose();
        }
    }

    public struct PacketHandleData 
    {
        public ENetClient Client { get; set; }
        public PacketReader Reader { get; set; }
    }
}
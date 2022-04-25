using Thread = System.Threading.Thread;

using GodotModules.Netcode;
using ENet;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules.Netcode.Client
{
    public abstract class ENetClient
    {
        public static uint PeerId { get; set; } // this clients peer id (grabbed from server at some point)
        public static CancellationTokenSource CancelTokenSource { get; private set; }
        public static ConsoleColor LogsColor = ConsoleColor.Yellow;
        public static ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        private static int OutgoingId { get; set; }
        private static ConcurrentDictionary<int, ClientPacket> Outgoing { get; set; }
        public static DisconnectOpcode DisconnectOpcode { get; set; }
        public static readonly Dictionary<ServerPacketOpcode, IPacketServer> HandlePacket = ReflectionUtils.LoadInstances<ServerPacketOpcode, IPacketServer>("SPacket");
        private static long Connected = 0;
        private static int ThreadLoops = 0;
        public static bool IsConnected { get => Interlocked.Read(ref Connected) == 1; } // thread safe
        private const int SetupThreadLoops = 10;
        public static bool IsSetup() => ThreadLoops == SetupThreadLoops;

        public static bool Running { get; set; }
        protected bool ENetThreadRunning;

        public ENetClient()
        {
            Running = false;
            Connected = 0;
            OutgoingId = 0;
            Outgoing = new ConcurrentDictionary<int, ClientPacket>();
            ENetCmds = new ConcurrentQueue<ENetCmd>();
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

            using (var client = new Host())
            {
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
                                CancelTokenSource.Cancel();
                                break;
                        }
                    }

                    // Outgoing
                    while (Outgoing.TryGetValue(OutgoingId--, out ClientPacket clientPacket))
                    {
                        Log("Sent packet: " + (ClientPacketOpcode)clientPacket.Opcode);
                        byte channelID = 0; // The channel all networking traffic will be going through
                        var packet = default(Packet);
                        packet.Create(clientPacket.Data, clientPacket.PacketFlags);
                        peer.Send(channelID, ref packet);
                    }

                    if (Connected == 1)
                        ThreadLoops = Mathf.Min(ThreadLoops + 1, SetupThreadLoops);

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

                                NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ENetPacket, new PacketReader(packet)));
                                break;

                            case EventType.Timeout:
                                HandlePeerLeave(DisconnectOpcode.Timeout);
                                Timeout(netEvent);
                                break;

                            case EventType.Disconnect:
                                HandlePeerLeave((DisconnectOpcode)netEvent.Data);
                                Disconnect(netEvent);
                                break;
                        }
                    }
                }

                client.Flush();
            }

            Library.Deinitialize();
            ENetThreadRunning = false;

            Log("Client stopped");

            while (ConcurrentQueuesWorking())
                await Task.Delay(100);

            Running = false;
        }

        private void HandlePeerLeave(DisconnectOpcode opcode)
        {
            SceneGameServers.ConnectingToLobby = false;
            SceneGameServers.Disconnected = true;
            Connected = 0;
            DisconnectOpcode = (DisconnectOpcode)opcode;
            System.Console.WriteLine("CLIENT THREAD: CHANGING SCENE TO GAMESERVERS");
            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ChangeScene, "GameServers"));
            CancelTokenSource.Cancel();
        }

        /// <summary>
        /// Send a packet to the server
        /// </summary>
        /// <param name="opcode">The opcode of the packet</param>
        /// <param name="data">The data if any</param>
        /// <returns></returns>
        public static async Task Send(ClientPacketOpcode opcode, IPacket data = null)
        {
            OutgoingId++;

            var success = Outgoing.TryAdd(OutgoingId, new ClientPacket((byte)opcode, data));

            if (!success)
                System.Console.WriteLine("FAILED TO ADD OUTGOING KEY");

            while (Outgoing.ContainsKey(OutgoingId))
                await Task.Delay(100);
        }

        private bool ConcurrentQueuesWorking() => NetworkManager.GodotCmds.Count != 0 || ENetCmds.Count != 0 || Outgoing.Count != 0;

        /// <summary>
        /// Attempt to connect to the server, can be called from the Godot thread
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async Task Connect(string ip, ushort port)
        {
            if (ENetThreadRunning)
            {
                SceneGameServers.ConnectingToLobby = false;
                Log("ENet thread is running already");
                return;
            }

            ENetThreadRunning = true;
            CancelTokenSource = new CancellationTokenSource();

            try
            {
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
        public async static Task Stop()
        {
            CancelTokenSource.Cancel();
            //ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToDisconnect));

            while (!CancelTokenSource.IsCancellationRequested)
                await Task.Delay(100);
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// Checks thread name, if its Client send request to log on Godot thread otherwise log on Godot thread directly
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void Log(object obj)
        {
            var threadName = Thread.CurrentThread.Name;

            if (threadName == "Client")
                NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessageClient, obj));
            else
                Utils.Log($"[Client]: {obj}", LogsColor);
        }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Connect(Event netEvent) { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Disconnect(Event netEvent) { }

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Timeout(Event netEvent) { }
    }
}
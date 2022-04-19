using Thread = System.Threading.Thread;

using Common.Netcode;
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
        public static Task WorkerClient { get; set; }
        public static ConsoleColor LogsColor = ConsoleColor.Yellow;
        public static bool Running;
        public static ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        public static ConcurrentQueue<GodotCmd> GodotCmds { get; set; }
        private static int OutgoingId { get; set; }
        private static ConcurrentDictionary<int, ClientPacket> Outgoing { get; set; }
        public static DisconnectOpcode DisconnectOpcode { get; set; }
        public static readonly Dictionary<ServerPacketOpcode, HandlePacket> HandlePacket = Utils.LoadInstances<ServerPacketOpcode, HandlePacket, ENetClient>();
        protected bool ENetThreadRunning;

        public ENetClient()
        {
            OutgoingId = 0;
            Outgoing = new ConcurrentDictionary<int, ClientPacket>();
            ENetCmds = new ConcurrentQueue<ENetCmd>();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
        }

        /// <summary>
        /// The client thread worker
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private async void ENetThreadWorker(string ip, ushort port)
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
                while (Running)
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
                                Running = false;
                                break;
                        }
                    }

                    // Outgoing
                    while (Outgoing.TryGetValue(OutgoingId--, out ClientPacket clientPacket))
                    {
                        byte channelID = 0; // The channel all networking traffic will be going through
                        var packet = default(Packet);
                        packet.Create(clientPacket.Data, clientPacket.PacketFlags);
                        peer.Send(channelID, ref packet);
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

                                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ENetPacket, new PacketReader(packet)));
                                break;

                            case EventType.Timeout:
                                DisconnectOpcode = DisconnectOpcode.Timeout;
                                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ChangeScene, "GameServers"));
                                SceneGameServers.ConnectingToLobby = false;
                                SceneGameServers.Disconnected = true;
                                Running = false;
                                Timeout(netEvent);
                                break;

                            case EventType.Disconnect:
                                DisconnectOpcode = (DisconnectOpcode)netEvent.Data;
                                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ChangeScene, "GameServers"));
                                SceneGameServers.ConnectingToLobby = false;
                                SceneGameServers.Disconnected = true;
                                Running = false;
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
        }

        /// <summary>
        /// Send a packet to the server
        /// </summary>
        /// <param name="opcode">The opcode of the packet</param>
        /// <param name="data">The data if any</param>
        /// <returns></returns>
        public static async Task Send(ClientPacketOpcode opcode, IWritable data = null)
        {
            OutgoingId++;

            Outgoing.TryAdd(OutgoingId, new ClientPacket((byte)opcode, data));

            while (Outgoing.ContainsKey(OutgoingId))
                await Task.Delay(100);
        }

        private bool ConcurrentQueuesWorking() => GodotCmds.Count != 0 || ENetCmds.Count != 0 || Outgoing.Count != 0;

        /// <summary>
        /// Attempt to connect to the server, can be called from the Godot thread
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async void Connect(string ip, ushort port)
        {
            if (ENetThreadRunning)
            {
                SceneGameServers.ConnectingToLobby = false;
                Log("ENet thread is running already");
                return;
            }

            ENetThreadRunning = true;

            try
            {
                WorkerClient = Task.Run(() => ENetThreadWorker(ip, port));
                await WorkerClient;
            }
            catch (Exception e)
            {
                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.PopupError, e));
                Utils.Log($"ENet Client: {e.Message}{e.StackTrace}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public async static Task Disconnect() => await Stop();

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public async static Task Stop()
        {
            ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToDisconnect));

            if (!ENetClient.WorkerClient.IsCompleted)
                await Task.Delay(100);
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// Checks thread name, if its Client send request to log on Godot thread otherwise log on Godot thread directly
        /// </summary>
        /// <param name="obj">The object to log</param>
        protected void Log(object obj) 
        {
            var threadName = Thread.CurrentThread.Name;

            if (threadName == "Client")
                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, obj));
            else
                Utils.Log($"{obj}", LogsColor);
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
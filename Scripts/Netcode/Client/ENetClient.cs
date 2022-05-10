using ENet;
using System;
using System.Collections.Concurrent;

using System.Threading;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace GodotModules.Netcode.Client
{
    public class ENetClient
    {
        public static readonly Dictionary<ServerPacketOpcode, APacketServer> HandlePacket = ReflectionUtils.LoadInstances<ServerPacketOpcode, APacketServer>("SPacket");

        public bool IsConnected { get => Interlocked.Read(ref Connected) == 1; }
        public bool IsRunning { get => Interlocked.Read(ref Running) == 1; }
        public ConcurrentQueue<ThreadCmd<ENetOpcode>> ENetCmds { get; set; }

        protected CancellationTokenSource CTSClientTask { get; set; }
        protected long Connected = 0;

        private bool IsENetThreadRunning { get => Interlocked.Read(ref ENetThreadRunning) == 1; }
        private long ENetThreadRunning = 0;
        private long Running = 0;
        private int OutgoingId { get; set; }
        private ConcurrentDictionary<int, ClientPacket> Outgoing { get; set; }

        public ENetClient()
        {
            Outgoing = new();
            ENetCmds = new();
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

        public async void Start(string ip, ushort port)
        {
            try
            {
                if (IsENetThreadRunning)
                {
                    NetworkManager.ClientConnectingToLobby = false;
                    Log($"Client is running already");
                    return;
                }

                ENetThreadRunning = 1;
                CTSClientTask = new CancellationTokenSource();

                await Task.Run(() => ENetThreadWorker(ip, port), CTSClientTask.Token);
            }
            catch (Exception e)
            {
                Logger.LogErr(e, "Client");
            }
        }

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public void Stop() => ENetCmds.Enqueue(new ThreadCmd<ENetOpcode>(ENetOpcode.ClientWantsToDisconnect));

        public void Log(object obj) => Logger.Log($"[Client]: {obj}", ConsoleColor.Yellow);

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

        private Task ENetThreadWorker(string ip, ushort port)
        {
            Thread.CurrentThread.Name = "Client";

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

            Running = 1;

            while (!CTSClientTask.IsCancellationRequested)
            {
                var polled = false;

                // ENet Cmds from Godot Thread
                while (ENetCmds.TryDequeue(out ThreadCmd<ENetOpcode> cmd))
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
                    NetworkManager.PingSent = DateTime.Now;
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

                            GameManager.GodotCommands.Enqueue(GodotOpcode.ENetPacket, new PacketReader(packet));
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

            ENetThreadRunning = 0;

            Log($"Client stopped");
            
            Running = 0;

            return Task.FromResult(1);
        }
    }

    public struct PacketHandleData 
    {
        public PacketReader Reader { get; set; }
    }
}
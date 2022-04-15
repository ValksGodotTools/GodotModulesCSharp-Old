using Common.Netcode;
using ENet;
using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer; // ambitious reference between Godot.Timer and System.Timers.Timer

namespace GodotModules.Netcode.Server
{
    public abstract class ENetServer : Node
    {
        public ConcurrentQueue<ENetCmd> ENetCmds { get; private set; }
        public ConcurrentQueue<GodotCmd> GodotCmds { get; private set; }
        public ConcurrentQueue<ServerPacket> Outgoing { get; private set; }
        public bool Running { get; private set; }

        private ConcurrentBag<Event> Incoming { get; set; }
        private Dictionary<uint, Peer> Peers { get; set; }
        private bool QueueRestart { get; set; }
        public Timer TimerPingMasterServer { get; set; }
        public static ENetServer Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            ENetCmds = new ConcurrentQueue<ENetCmd>();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
            Outgoing = new ConcurrentQueue<ServerPacket>();
            Incoming = new ConcurrentBag<Event>();
            Peers = new Dictionary<uint, Peer>();
            TimerPingMasterServer = new Timer(WebClient.WEB_PING_INTERVAL);
            TimerPingMasterServer.AutoReset = true;
            TimerPingMasterServer.Elapsed += new ElapsedEventHandler(WebClient.OnTimerPingMasterServerEvent);
        }

        public override void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.LogMessage:
                        GD.Print((string)cmd.Data);
                        return;

                    case GodotOpcode.AddPlayerToLobbyList:
                        //UILobby.AddPlayer((string)cmd.Data);
                        // TODO: Find another way to add player to lobby
                        return;
                }
            }
        }

        /// <summary>
        /// The server thread worker
        /// </summary>
        /// <param name="port"></param>
        /// <param name="maxClients"></param>
        public Task ENetThreadWorker(ushort port, int maxClients)
        {
            Running = true;
            if (UIGameServers.Instance.CurrentLobby.Public)
                TimerPingMasterServer.Start();

            Library.Initialize();

            using (Host server = new Host())
            {
                Address address = new Address();

                address.Port = port;
                server.Create(address, maxClients);

                GDLog($"Server listening on port {port}");

                while (Running)
                {
                    bool polled = false;

                    // ENet Cmds
                    while (ENetCmds.TryDequeue(out ENetCmd cmd)) 
                    {
                        //var opcode = cmd.Opcode;
                    }

                    // Incoming
                    while (Incoming.TryTake(out Event netEvent))
                    {
                        var packet = netEvent.Packet;
                        var packetReader = new PacketReader(packet);
                        var opcode = (ClientPacketOpcode)packetReader.ReadByte();

                        GDLog($"Received New Client Packet: {opcode}");

                        Receive(netEvent, opcode, packetReader);

                        packetReader.Dispose();
                    }

                    // Outgoing
                    while (Outgoing.TryDequeue(out ServerPacket packet))
                    {
                        foreach (var peer in packet.Peers)
                            Send(packet, peer);
                    }

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
                                GDLog($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
                                packet.Dispose();
                                continue;
                            }

                            Incoming.Add(netEvent);
                        }
                        else if (eventType == EventType.Connect)
                        {
                            // Connect
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
            }

            GDLog("Server stopped");
            Stopped();

            if (QueueRestart)
            {
                QueueRestart = false;
                Start();
            }

            return Task.FromResult(1);
        }

        /// <summary>
        /// Start the server, can be called from the Godot thread
        /// </summary>
        public async void Start()
        {
            if (Running)
            {
                GDLog("Server is running already");
                return;
            }

            GDLog("Attempting to connect to server");

            try 
            {
                var workerServer = Task.Run(() => ENetThreadWorker(25565, 100));
                await workerServer;
            } 
            catch (Exception e)
            {
                GD.Print($"Worker server: {e.Message}");
            }
        }

        /// <summary>
        /// Stop the server, can be called from the Godot thread
        /// </summary>
        public void Stop()
        {
            if (!Running)
            {
                GDLog("Server has been stopped already");
                return;
            }

            DisconnectAllPeers();

            GDLog("Stopping server");
            Running = false;
        }

        /// <summary>
        /// Restart the server, can be called from the Godot thread
        /// </summary>
        public void Restart()
        {
            if (!Running)
            {
                GDLog("Server has been stopped already");
                return;
            }

            DisconnectAllPeers();

            GDLog("Restarting server");
            Running = false;
            QueueRestart = true;
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// </summary>
        /// <param name="obj">The object to log</param>
        public void GDLog(object obj) => GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.LogMessage, Data = obj });

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected abstract void Connect(Event netEvent);

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected abstract void Disconnect(Event netEvent);

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected abstract void Timeout(Event netEvent);

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="opcode"></param>
        /// <param name="reader"></param>
        protected abstract void Receive(Event netEvent, ClientPacketOpcode opcode, PacketReader reader);
        
        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        protected abstract void Stopped();

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
        /// Kick all peers from the server, a method to be used only on the ENet thread
        /// </summary>
        private void DisconnectAllPeers()
        {
            foreach (var peer in Peers.Values)
                peer.DisconnectNow(0);

            Peers.Clear();
        }
    }
}
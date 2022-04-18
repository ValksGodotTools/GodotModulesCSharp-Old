using Common.Netcode;
using ENet;
using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace GodotModules.Netcode.Server
{
    public abstract class ENetServer
    {
        public static Task WorkerServer { get; set; }
        public static bool Running { get; set; }
        public static ConcurrentQueue<ServerPacket> Outgoing { get; set; }
        public ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        public static Dictionary<uint, Peer> Peers { get; set; }
        public static ConcurrentQueue<GodotCmd> GodotCmds { get; set; }
        private static readonly Dictionary<ClientPacketOpcode, HandlePacket> HandlePacket = Utils.LoadInstances<ClientPacketOpcode, HandlePacket, ENetServer>();
        private bool QueueRestart { get; set; }

        public ENetServer()
        {
            ENetCmds = new ConcurrentQueue<ENetCmd>();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
            Outgoing = new ConcurrentQueue<ServerPacket>();
            Peers = new Dictionary<uint, Peer>();
        }

        /// <summary>
        /// The server thread worker
        /// </summary>
        /// <param name="port"></param>
        /// <param name="maxClients"></param>
        public async void ENetThreadWorker(ushort port, int maxClients)
        {
            Running = true;
            if (UILobby.CurrentLobby.Public)
                GameManager.WebClient.TimerPingMasterServer.Start();

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
                        var opcode = cmd.Opcode;

                        if (opcode == ENetOpcode.ClientWantsToExitApp)
                        {
                            Running = false;
                            DisconnectAllPeers();
                        }
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

                            var packetReader = new PacketReader(packet);
                            var opcode = (ClientPacketOpcode)packetReader.ReadByte();

                            GDLog($"Received new client packet: {opcode}");
                            HandlePacket[opcode].Handle(netEvent.Peer, packetReader);

                            packetReader.Dispose();
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

            while (ConcurrentQueuesWorking())
                await Task.Delay(100);

            Stopped();

            if (QueueRestart)
            {
                QueueRestart = false;
                //Start();
                GameManager.StartServer();
            }
        }

        private bool ConcurrentQueuesWorking() => GodotCmds.Count != 0 || ENetCmds.Count != 0 || Outgoing.Count != 0;

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

            GDLog("Starting server");

            try
            {
                WorkerServer = Task.Run(() => ENetThreadWorker(25565, 100));
                await WorkerServer;
            }
            catch (Exception e)
            {
                GD.Print($"ENet Server: {e.Message}{e.StackTrace}");
            }
        }

        /// <summary>
        /// Stop the server, can be called from the Godot thread
        /// </summary>
        public static async Task Stop()
        {
            if (!Running)
            {
                GDLog("Server has been stopped already");
                return;
            }

            DisconnectAllPeers();

            GDLog("Stopping server");
            Running = false;

            if (!ENetServer.WorkerServer.IsCompleted)
                await Task.Delay(100);
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

        protected static Peer[] GetOtherPeers(uint id)
        {
            var otherPeers = new Dictionary<uint, Peer>(Peers);
            otherPeers.Remove(id);
            return otherPeers.Values.ToArray();
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// </summary>
        /// <param name="obj">The object to log</param>
        public static void GDLog(object obj) => GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, obj));

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

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        protected virtual void Stopped() { }

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
        private static void DisconnectAllPeers()
        {
            foreach (var peer in Peers.Values)
                peer.DisconnectNow(0);

            Peers.Clear();
        }
    }
}
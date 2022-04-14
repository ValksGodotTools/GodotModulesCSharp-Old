using Timer = System.Timers.Timer; // ambitious reference between Godot.Timer and System.Timers.Timer

using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Netcode;
using ENet;
using Godot;
using Common.Game;
using Valk.Modules.Netcode;

namespace Valk.Modules.Netcode.Server
{
    public abstract class ENetServer : Node
    {
        public static ConcurrentQueue<ENetCommand> ENetCmds { get; private set; }
        public static ConcurrentQueue<GodotCmd> GodotCmds { get; private set; }
        public static ConcurrentQueue<ServerPacket> Outgoing { get; private set; }
        public static bool Running { get; private set; }

        private static ConcurrentBag<Event> Incoming { get; set; }
        private static Dictionary<ENetOpcode, ENetCmd> ENetCmd { get; set; }
        private static Dictionary<ClientPacketOpcode, HandlePacket> HandlePacket { get; set; }
        private static Dictionary<uint, Peer> Peers { get; set; }
        private static bool QueueRestart { get; set; }
        public static Timer TimerPingMasterServer { get; set; }

        public override void _Ready()
        {
            ENetCmds = new ConcurrentQueue<ENetCommand>();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
            Outgoing = new ConcurrentQueue<ServerPacket>();
            Incoming = new ConcurrentBag<Event>();
            ENetCmd = typeof(ENetCmd).Assembly.GetTypes().Where(x => typeof(ENetCmd).IsAssignableFrom(x) && !x.IsAbstract).Select(Activator.CreateInstance).Cast<ENetCmd>().ToDictionary(x => x.Opcode, x => x);
            HandlePacket = typeof(HandlePacket).Assembly.GetTypes().Where(x => typeof(HandlePacket).IsAssignableFrom(x) && !x.IsAbstract).Select(Activator.CreateInstance).Cast<HandlePacket>().ToDictionary(x => x.Opcode, x => x);
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
                        GD.Print((string)cmd.Data[0]);
                        return;
                }
            }
        }

        public void Start()
        {
            if (Running)
            {
                GDLog("Server is running already");
                return;
            }

            GDLog("Attempting to connect to server");

            Task.Run(() => ENetThreadWorker(25565, 100));
        }

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

        public void ENetThreadWorker(ushort port, int maxClients)
        {
            Running = true;
            if (UIGameServers.CurrentLobby.Public)
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
                    while (ENetCmds.TryDequeue(out ENetCommand cmd))
                        ENetCmd[cmd.Opcode].Handle(cmd.Data);

                    // Incoming
                    while (Incoming.TryTake(out Event netEvent))
                    {
                        var packet = netEvent.Packet;
                        var packetReader = new PacketReader(packet);
                        var opcode = (ClientPacketOpcode)packetReader.ReadByte();

                        GDLog($"Received New Client Packet: {opcode}");

                        HandlePacket[opcode].Handle(netEvent.Peer, packetReader); // is ref needed for the packetReader param?
                        packetReader.Dispose(); // is this right?
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

            if (QueueRestart)
            {
                QueueRestart = false;
                Start();
            }
        }

        protected abstract void Connect(Event netEvent);
        protected abstract void Disconnect(Event netEvent);
        protected abstract void Timeout(Event netEvent);
        protected static void GDLog(string text) => GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.LogMessage, Data = new List<object> { text } });

        private void Send(ServerPacket gamePacket, Peer peer)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, gamePacket.PacketFlags);
            byte channelID = 0;
            peer.Send(channelID, ref packet);
        }

        private void DisconnectAllPeers()
        {
            foreach (var peer in Peers.Values)
                peer.DisconnectNow(0);

            Peers.Clear();
        }
    }

    public class ENetCommand
    {
        public ENetOpcode Opcode { get; set; }
        public List<object> Data { get; set; }
    }

    public enum ENetOpcode
    {
        GetOnlinePlayers,
        GetPlayerStats,
        KickPlayer,
        BanPlayer,
        PardonPlayer,
        ClearPlayerStats,
        SendPlayerData
    }

    public class GodotCmd
    {
        public GodotOpcode Opcode { get; set; }
        public List<object> Data { get; set; }
    }

    public enum GodotOpcode
    {
        LogMessage
    }
}
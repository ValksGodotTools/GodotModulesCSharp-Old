using Common.Netcode;
using ENet;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GodotModules.Netcode.Client
{
    public abstract class ENetClient : Node
    {
        public static Task WorkerClient { get; set; }
        public static bool Running;
        public ConcurrentQueue<ClientPacket> Outgoing { get; set; }
        public ConcurrentQueue<ENetCmd> ENetCmds { get; set; }
        protected bool ENetThreadRunning;
        private static readonly Dictionary<ServerPacketOpcode, HandlePacket> HandlePacket = Utils.LoadInstances<ServerPacketOpcode, HandlePacket, ENetClient>();
        private ConcurrentQueue<GodotCmd> GodotCmds { get; set; }

        public override void _Ready()
        {
            Outgoing = new ConcurrentQueue<ClientPacket>();
            ENetCmds = new ConcurrentQueue<ENetCmd>();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
        }

        public override void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        GDLog($"Received new server packet: {opcode}");

                        HandlePacket[opcode].Handle(packetReader);

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessage:
                        GD.Print($"[Client]: {cmd.Data}");
                        break;
                }

                ProcessGodotCommands(cmd);
            }
        }

        /// <summary>
        /// The client thread worker
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private async void ENetThreadWorker(string ip, ushort port)
        {
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
                    while (Outgoing.TryDequeue(out ClientPacket clientPacket))
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
                                    GDLog($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
                                    packet.Dispose();
                                    continue;
                                }

                                GodotCmds.Enqueue(new GodotCmd(GodotOpcode.ENetPacket, new PacketReader(packet)));
                                break;

                            case EventType.Timeout:
                                Running = false;
                                Timeout(netEvent);
                                break;

                            case EventType.Disconnect:
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

            GDLog("Client stopped");

            while (ConcurrentQueuesWorking())
                await Task.Delay(100);

            GameManager.GameClient.QueueFree();
            GameManager.GameClient = null;
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
                GD.Print("ENet thread is running already");
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
                GD.Print($"ENet Client: {e.Message}{e.StackTrace}");
            }
        }

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public async Task Stop() 
        {
            ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToDisconnect));

            if (!ENetClient.WorkerClient.IsCompleted)
                await Task.Delay(100);
        }

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// </summary>
        /// <param name="obj">The object to log</param>
        protected void GDLog(object obj) => GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, obj));

        /// <summary>
        /// This is in the Godot thread, anything from the Godot thread can be used here
        /// </summary>
        /// <param name="cmd">A command received from the ENet thread</param>
        protected virtual void ProcessGodotCommands(GodotCmd cmd) {}

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Connect(Event netEvent) {}

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Disconnect(Event netEvent) {}

        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="netEvent"></param>
        protected virtual void Timeout(Event netEvent) {}
    }
}
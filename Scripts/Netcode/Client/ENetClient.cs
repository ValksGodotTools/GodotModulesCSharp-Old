using Common.Netcode;
using ENet;
using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GodotModules.Netcode.Client
{
    public abstract class ENetClient : Node
    {
        public static readonly ConcurrentQueue<ClientPacket> Outgoing = new ConcurrentQueue<ClientPacket>();
        public static readonly ConcurrentQueue<ENetCmd> ENetCmds = new ConcurrentQueue<ENetCmd>();
        private static readonly ConcurrentBag<Packet> Incoming = new ConcurrentBag<Packet>();
        private static readonly ConcurrentQueue<GodotCmd> GodotCmds = new ConcurrentQueue<GodotCmd>();
        public static bool ENetThreadRunning;
        private static bool RunningNetCode;

        public override void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        Receive(opcode, packetReader);

                        packetReader.Dispose();
                        return;

                    case GodotOpcode.LogMessage:
                        GD.Print((string)cmd.Data);
                        return;

                    case GodotOpcode.ExitApp:
                        GetTree().Quit();
                        return;
                }

                ProcessGodotCommands(cmd);
            }
        }

        /// <summary>
        /// The client thread worker
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private Task ENetThreadWorker(string ip, ushort port)
        {
            Library.Initialize();
            var wantsToExit = false;
            var wantsToDisconnect = false;

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

                RunningNetCode = true;
                while (RunningNetCode)
                {
                    var polled = false;

                    // ENet Cmds from Godot Thread
                    while (ENetCmds.TryDequeue(out ENetCmd cmd))
                    {
                        switch (cmd.Opcode)
                        {
                            case ENetOpcode.ClientWantsToExitApp:
                                peer.Disconnect(0);
                                RunningNetCode = false;
                                wantsToExit = true;
                                break;

                            case ENetOpcode.ClientWantsToDisconnect:
                                peer.Disconnect(0);
                                RunningNetCode = false;
                                wantsToDisconnect = true;
                                break;
                        }
                    }

                    // Incoming
                    while (Incoming.TryTake(out Packet packet))
                        GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.ENetPacket, Data = new PacketReader(packet) });

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

                                Incoming.Add(netEvent.Packet);
                                break;

                            case EventType.Timeout:
                                RunningNetCode = false;
                                wantsToDisconnect = true;
                                Timeout(netEvent);
                                break;

                            case EventType.Disconnect:
                                RunningNetCode = false;
                                wantsToDisconnect = true;
                                Disconnect(netEvent);
                                break;
                        }
                    }
                }

                client.Flush();
            }

            Library.Deinitialize();
            ENetThreadRunning = false;

            if (wantsToDisconnect)
                GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.LoadMainMenu });

            if (wantsToExit)
                GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.ExitApp });

            return Task.FromResult(1);
        }

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
                var workerClient = Task.Run(() => ENetThreadWorker(ip, port));
                await workerClient;
            }
            catch (Exception e)
            {
                GD.Print($"Worker client: {e.Message}");
            }
        }

        /// <summary>
        /// Disconnect the client from the server, can be called from the Godot thread
        /// </summary>
        public void Disconnect() => ENetCmds.Enqueue(new ENetCmd { Opcode = ENetOpcode.ClientWantsToDisconnect });

        /// <summary>
        /// Provides a way to log a message on the Godot thread from the ENet thread
        /// </summary>
        /// <param name="obj">The object to log</param>
        protected static void GDLog(object obj) => GodotCmds.Enqueue(new GodotCmd { Opcode = GodotOpcode.LogMessage, Data = obj });

        /// <summary>
        /// This is in the Godot thread, anything from the Godot thread can be used here
        /// </summary>
        /// <param name="cmd">A command received from the ENet thread</param>
        protected abstract void ProcessGodotCommands(GodotCmd cmd);

        /// <summary>
        /// This is in the Godot thread, anything from the Godot thread can be used here
        /// </summary>
        /// <param name="opcode">The opcode received from the server</param>
        /// <param name="reader">The data received from the server</param>
        protected abstract void Receive(ServerPacketOpcode opcode, PacketReader reader);

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
    }
}
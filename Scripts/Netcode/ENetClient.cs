using ENet;
using System.Threading;

namespace GodotModules.Netcode.Client 
{
    public abstract class ENetClient 
    {
        public static readonly Dictionary<ServerPacketOpcode, APacketServer> HandlePacket = ReflectionUtils.LoadInstances<ServerPacketOpcode, APacketServer>("SPacket");

        public bool IsConnected { get => Interlocked.Read(ref _connected) == 1; }
        public bool IsRunning { get => Interlocked.Read(ref _running) == 1; }
        public ConcurrentQueue<ENetCmd> ENetCmds = new ConcurrentQueue<ENetCmd>();

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        private long _connected;
        private long _running;
        private ConcurrentDictionary<int, ClientPacket> _outgoing = new ConcurrentDictionary<int, ClientPacket>();
        private int _outgoingId;

        public async Task Send(ClientPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            _outgoingId++;
            var success = _outgoing.TryAdd(_outgoingId, new ClientPacket((byte)opcode, flags, data));

            if (!success)
                Log($"Failed to add {opcode} to Outgoing queue because of duplicate key");

            while (_outgoing.ContainsKey(_outgoingId))
                await Task.Delay(1);
        }

        protected virtual void Connect(Event netEvent) {}
        protected virtual void Disconnect(Event netEvent) {}
        protected virtual void Timeout(Event netEvent) {}

        private Task ENetThreadWorker(string ip, ushort port)
        {
            using var client = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            client.Create();

            var peer = client.Connect(address);

            uint pingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
            uint timeout = 5000; // Will be ignored if maximum timeout is exceeded
            uint timeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
            uint timeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

            peer.PingInterval(pingInterval);
            peer.Timeout(timeout, timeoutMinimum, timeoutMaximum);

            _running = 1;

            while (!CancellationTokenSource.IsCancellationRequested)
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
                while (_outgoing.TryRemove(_outgoingId, out ClientPacket clientPacket))
                {
                    _outgoingId--;
                    byte channelID = 0; // The channel all networking traffic will be going through
                    var packet = default(Packet);
                    packet.Create(clientPacket.Data, clientPacket.PacketFlags);
                    //Log("Sent packet: " + (ClientPacketOpcode)clientPacket.Opcode);
                    peer.Send(channelID, ref packet);
                    GM.NetworkManager.PingSent = DateTime.Now;
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
                            _connected = 1;
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

                            GM.GodotCmd(GodotOpcode.ENetPacket, new PacketReader(packet));
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

            Log($"Client stopped");
            
            _running = 0;

            return Task.FromResult(1);
        }

        public void Log(object v) => GM.Log($"[Client]: {v}", ConsoleColor.Yellow);
    }

    public struct ENetCmd 
    {
        public ENetOpcode Opcode { get; set; }
        public object Data { get; set; }

        public ENetCmd(ENetOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }
    }

    public enum ENetOpcode 
    {
        ClientWantsToExitApp,
        ClientWantsToDisconnect
    }
}
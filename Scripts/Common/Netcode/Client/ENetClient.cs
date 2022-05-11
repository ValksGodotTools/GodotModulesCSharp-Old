using ENet;
using System.Threading;

namespace GodotModules.Netcode.Client 
{
    public abstract class ENetClient : IDisposable
    {
        public static readonly Dictionary<ServerPacketOpcode, APacketServer> HandlePacket = ReflectionUtils.LoadInstances<ServerPacketOpcode, APacketServer>("SPacket");

        public bool IsConnected { get => Interlocked.Read(ref _connected) == 1; }
        public bool IsRunning { get => Interlocked.Read(ref _running) == 1; }

        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private ConcurrentQueue<ENetClientCmd> ENetCmds = new ConcurrentQueue<ENetClientCmd>();
        private long _connected;
        private long _running;
        private ConcurrentDictionary<int, ClientPacket> _outgoing = new ConcurrentDictionary<int, ClientPacket>();
        private int _outgoingId;

        public async void Start(string ip, ushort port) => await StartAsync(ip, port);

        public async Task StartAsync(string ip, ushort port)
        {
            try
            {
                if (IsRunning)
                {
                    GM.Log($"Client is running already");
                    return;
                }

                _running = 1;
                CancellationTokenSource = new CancellationTokenSource();

                await Task.Run(() => ENetThreadWorker(ip, port), CancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                GM.LogErr(e, "Client");
            }
        }

        public void Stop() => ENetCmds.Enqueue(new ENetClientCmd(ENetClientOpcode.Disconnect));

        public async Task StopAsync()
        {
            Stop();

            while (IsRunning)
                await Task.Delay(1);
        }

        public void Send(ClientPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable) 
        {
            _outgoingId++;
            var success = _outgoing.TryAdd(_outgoingId, new ClientPacket((byte)opcode, flags, data));

            if (!success)
                GM.LogWarning($"Failed to add {opcode} to Outgoing queue because of duplicate key");
        }

        public async Task SendAsync(ClientPacketOpcode opcode, APacket data = null, PacketFlags flags = PacketFlags.Reliable)
        {
            Send(opcode, data, flags);

            while (_outgoing.ContainsKey(_outgoingId))
                await Task.Delay(1);
        }

        protected virtual void Connecting() {}
        protected virtual void Connect(Event netEvent) {}
        protected virtual void Disconnect(Event netEvent) {}
        protected virtual void Timeout(Event netEvent) {}
        protected virtual void Leave(Event netEvent) {}
        protected virtual void Sent(ClientPacketOpcode opcode) {}
        protected virtual void Stopped() {}

        private Task ENetThreadWorker(string ip, ushort port)
        {
            using var client = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            client.Create();

            Connecting();
            var peer = client.Connect(address);

            uint pingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
            uint timeout = 5000; // Will be ignored if maximum timeout is exceeded
            uint timeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
            uint timeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

            peer.PingInterval(pingInterval);
            peer.Timeout(timeout, timeoutMinimum, timeoutMaximum);

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var polled = false;

                // ENet Cmds from Godot Thread
                while (ENetCmds.TryDequeue(out ENetClientCmd cmd))
                {
                    switch (cmd.Opcode)
                    {
                        case ENetClientOpcode.Disconnect:
                            if (CancellationTokenSource.IsCancellationRequested)
                            {
                                GM.LogWarning("Client is in the middle of stopping");
                                break;
                            }

                            CancellationTokenSource.Cancel();
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
                    peer.Send(channelID, ref packet);
                    Sent((ClientPacketOpcode)clientPacket.Opcode);
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
                                GM.LogWarning($"Tried to read packet from server of size {packet.Length} when max packet size is {GamePacket.MaxSize}");
                                packet.Dispose();
                                continue;
                            }

                            GM.GodotCmd(GodotOpcode.ENetPacket, new PacketReader(packet));
                            break;

                        case EventType.Timeout:
                            CancellationTokenSource.Cancel();
                            Timeout(netEvent);
                            Leave(netEvent);
                            break;

                        case EventType.Disconnect:
                            CancellationTokenSource.Cancel();
                            Disconnect(netEvent);
                            Leave(netEvent);
                            break;
                    }
                }

                client.Flush();
            }
            
            _running = 0;

            Stopped();

            return Task.FromResult(1);
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }

    public class ENetClientCmd 
    {
        public ENetClientOpcode Opcode { get; set; }
        public object Data { get; set; }

        public ENetClientCmd(ENetClientOpcode opcode, object data = null)
        {
            Opcode = opcode;
            Data = data;
        }
    }

    public enum ENetClientOpcode 
    {
        Disconnect
    }
}
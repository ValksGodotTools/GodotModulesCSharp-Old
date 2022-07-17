using LiteNetLib;
using LiteNetLib.Utils;

namespace GodotModules.Netcode.Server
{
    using Event = ENet.Event;
    
    public class GameServer : ENetServer
    {
        public Dictionary<byte, DataPlayer> Players = new();
        public DataLobby Lobby { get; set; }

        public GameServer(Net networkManager) : base(networkManager) {}

        public Dictionary<byte, DataPlayer> GetOtherPlayers(byte id)
        {
            var otherPlayers = new Dictionary<byte, DataPlayer>(Players);
            otherPlayers.Remove(id);
            return otherPlayers;
        }

        public NetPeer[] GetOtherPlayerPeers(uint id) => Players.Keys.Where(x => x != id).Select(x => Peers[x]).ToArray();

        public NetPeer[] GetAllPlayerPeers() => Players.Keys.Select(x => Peers[x]).ToArray();

        public void SendToAllPlayers(ServerPacketOpcode opcode, APacket data = null, DeliveryMethod flags = DeliveryMethod.ReliableOrdered)
        {
            var allPlayers = GetAllPlayerPeers();

            if (data == null)
                Send(opcode, flags, allPlayers);
            else
                Send(opcode, data, flags, allPlayers);
        }

        public void SendToOtherPeers(uint id, ServerPacketOpcode opcode, APacket data = null, DeliveryMethod flags = DeliveryMethod.ReliableOrdered)
        {
            var otherPeers = GetOtherPeers(id);
            if (otherPeers.Length == 0)
                return;

            if (data == null)
                Send(opcode, flags, otherPeers);
            else
                Send(opcode, data, flags, otherPeers);
        }

        public void SendToOtherPlayers(uint id, ServerPacketOpcode opcode, APacket data = null, DeliveryMethod flags = DeliveryMethod.ReliableOrdered)
        {
            var otherPlayers = GetOtherPlayerPeers(id);
            if (otherPlayers.Length == 0)
                return;

            if (data == null)
                Send(opcode, flags, otherPlayers);
            else
                Send(opcode, data, flags, otherPlayers);
        }

        protected override void ServerCmds()
        {
            while (ENetCmds.TryDequeue(out ENetServerCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case ENetServerOpcode.Stop:
                        if (CancellationTokenSource.IsCancellationRequested) 
                        {
                            Log("Server is in the middle of stopping");
                            break;
                        }

                        KickAll(DisconnectOpcode.Stopping);
                        CancellationTokenSource.Cancel();
                        break;

                    case ENetServerOpcode.Restart:
                        if (CancellationTokenSource.IsCancellationRequested)
                        {
                            Log("Server is in the middle of restarting");
                            break;
                        }

                        KickAll(DisconnectOpcode.Restarting);
                        CancellationTokenSource.Cancel();
                        _queueRestart = true;
                        break;
                }
            }
        }

        protected override void Started(ushort port, int maxClients)
        {
            Log($"Server listening on port {port}");
        }

        protected override void Connect(ref Event netEvent)
        {
            Log($"Client connected with id: {netEvent.Peer.ID}");
        }

        protected override void Received(NetPeer peer, PacketReader packetReader, ClientPacketOpcode opcode)
        {
            /*Log($"Received: {opcode}");

            if (!HandlePacket.ContainsKey(opcode))
            {
                Logger.LogWarning($"[Server]: Received malformed opcode: {opcode} (Ignoring)");
                return;
            }

            var handlePacket = HandlePacket[opcode];
            try
            {
                handlePacket.Read(packetReader);
            }
            catch (System.IO.EndOfStreamException e)
            {
                Logger.LogWarning($"[Server]: Received malformed opcode: {opcode} {e.Message} (Ignoring)");
                return;
            }
            handlePacket.Handle(this, peer);*/
        }

        protected override void Disconnect(ref Event netEvent)
        {
            Log($"Client disconnected with id: {netEvent.Peer.ID}");
        }

        protected override void Timeout(ref Event netEvent)
        {
            Log($"Client timed out with id: {netEvent.Peer.ID}");
        }

        protected override void Leave(ref Event netEvent)
        {
            Players.Remove((byte)netEvent.Peer.ID);
        }

        protected override void Stopped()
        {
            Log("Server stopped");
        }
    }
}
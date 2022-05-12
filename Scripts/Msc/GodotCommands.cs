using GodotModules.Netcode.Client;

namespace GodotModules
{
    public class GodotCommands
    {
        private ConcurrentQueue<GodotCmd> _godotCmdQueue = new ConcurrentQueue<GodotCmd>();
        private NetworkManager _networkManager;

        public GodotCommands(NetworkManager networkManager) 
        {
            _networkManager = networkManager;
        }

        public void Enqueue(GodotOpcode opcode, object data = null) => _godotCmdQueue.Enqueue(new GodotCmd(opcode, data));

        public async Task Update()
        {
            if (_godotCmdQueue.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //GM.Log($"[Client]: Received {opcode}");

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        handlePacket.Read(packetReader);

                        await handlePacket.Handle();

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.ChangeScene:
                        await GM.ChangeScene($"{cmd.Data}");
                        break;

                    case GodotOpcode.Disconnect:
                        _networkManager.DisconnectOpcode = (DisconnectOpcode)cmd.Data;
                        await GM.ChangeScene("GameServers");
                        break;
                }
            }
        }
    }

    public class GodotCmd 
    {
        public GodotOpcode Opcode { get; set; }
        public object Data { get; set; }

        public GodotCmd(GodotOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }
    }

    public enum GodotOpcode 
    {
        ENetPacket,
        ChangeScene,
        Disconnect
    }
} 
using GodotModules.Netcode.Client;

namespace GodotModules
{
    public class GodotCommands
    {
        private readonly ConcurrentQueue<GodotCmd> _godotCmdQueue = new ConcurrentQueue<GodotCmd>();
        private readonly NetworkManager _networkManager;

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
                        var packetInfo = (PacketInfo)cmd.Data;
                        var packetReader = packetInfo.PacketReader;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //Logger.Log($"[Client]: Received {opcode}");

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        handlePacket.Read(packetReader);

                        await handlePacket.Handle(packetInfo.GameClient);

                        packetReader.Dispose();
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
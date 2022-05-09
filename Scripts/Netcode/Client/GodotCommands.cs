using GodotModules.Netcode.Client;

namespace GodotModules
{
    public static class GodotCommands
    {
        private static ConcurrentQueue<ThreadCmd<GodotOpcode>> GodotCmdQueue = new ConcurrentQueue<ThreadCmd<GodotOpcode>>();

        public static void Enqueue(GodotOpcode opcode, object data = null) => GodotCmdQueue.Enqueue(new ThreadCmd<GodotOpcode>(opcode, data));

        public static async Task Dequeue()
        {
            if (GodotCmdQueue.TryDequeue(out ThreadCmd<GodotOpcode> cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //Utils.Log($"[Client]: Received {opcode}");

                        if (!ENetClient.HandlePacket.ContainsKey(opcode))
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException ex)
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} {ex.Message} (Ignoring)");
                            break;
                        }
                        await handlePacket.Handle();

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.PopupMessage:
                        GameManager.SpawnPopupMessage((string)cmd.Data);
                        break;

                    case GodotOpcode.ChangeScene:
                        await SceneManager.ChangeScene($"{cmd.Data}");
                        break;

                    case GodotOpcode.Disconnect:
                        NetworkManager.DisconnectOpcode = (DisconnectOpcode)cmd.Data;
                        await SceneManager.ChangeScene("GameServers");
                        break;
                }
            }
        }
    }
}
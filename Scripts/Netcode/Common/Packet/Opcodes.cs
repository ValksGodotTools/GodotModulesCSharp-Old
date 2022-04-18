namespace Common.Netcode
{
    // Received from Game Client
    public enum ClientPacketOpcode
    {
        LobbyJoin
    }

    // Sent to Game Client
    public enum ServerPacketOpcode
    {
        LobbyJoin,
        LobbyList
    }

    public enum DisconnectOpcode
    {
        Disconnected,
        Maintenance,
        Restarting,
        Kicked,
        Banned,
        PlayerWithUsernameExistsOnServerAlready
    }

    public class GodotCmd
    {
        public GodotCmd(GodotOpcode opcode)
        {
            Opcode = opcode;
        }

        public GodotCmd(GodotOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }

        public GodotOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum GodotOpcode
    {
        ENetPacket,
        LogMessage
    }

    public class ENetCmd
    {
        public ENetCmd(ENetOpcode opcode) 
        {
            Opcode = opcode;
        }

        public ENetCmd(ENetOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }

        public ENetOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum ENetOpcode
    {
        ClientWantsToExitApp,
        ClientWantsToDisconnect
    }
}
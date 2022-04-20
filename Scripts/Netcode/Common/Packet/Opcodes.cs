namespace GodotModules.Netcode
{
    // Received from Game Client
    public enum ClientPacketOpcode
    {
        LobbyJoin,
        LobbyLeave
    }

    // Sent to Game Client
    public enum ServerPacketOpcode
    {
        LobbyJoin,
        LobbyLeave,
        LobbyInfo
    }

    public enum DisconnectOpcode
    {
        Disconnected,
        Timeout,
        Maintenance,
        Restarting,
        Stopping,
        Kicked,
        Banned
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
        LogMessage,
        ChangeScene,
        PopupMessage,
        PopupError
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
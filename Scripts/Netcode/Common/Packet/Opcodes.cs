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
        public GodotOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum GodotOpcode
    {
        ENetPacket,
        LogMessage,
        LoadMainMenu,
        ExitApp,
        AddPlayerToLobbyList
    }

    public class ENetCmd
    {
        public ENetOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum ENetOpcode
    {
        ClientWantsToExitApp,
        ClientWantsToDisconnect,
        ClearPlayerStats
    }
}
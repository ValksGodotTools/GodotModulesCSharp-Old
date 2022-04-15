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
        ASD
    }

    public enum ResponseChannelCreateOpcode 
    {
        ChannelExistsAlready,
        Success
    }

    public enum PurchaseItemResponseOpcode
    {
        Purchased,
        NotEnoughResources
    }

    public enum JoinLeaveOpcode 
    {
        Join,
        Leave
    }

    public enum LoginResponseOpcode
    {
        LoginSuccessReturningPlayer,
        LoginSuccessNewPlayer,
        VersionMismatch,
        InvalidToken
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

    public struct Version
    {
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Patch { get; set; }
    }
}

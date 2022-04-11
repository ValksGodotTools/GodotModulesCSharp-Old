namespace Common.Netcode
{
    // Received from Game Client
    public enum ClientPacketOpcode
    {
        Disconnect,
        PurchaseItem,
        CreateAccount,
        Login,
        ChatMessage,
        CreateChannel,
        RemoveChannel,
        AddUserToChannel,
        RemoveUserFromChannel,
        AddFriend,
        Block
    }

    // Sent to Game Client
    public enum ServerPacketOpcode
    {
        ClientDisconnected,
        PurchasedItem,
        CreatedAccount,
        LoginResponse,
        PlayerData,
        ChatMessage,
        PlayerJoinLeave,
        PlayerList,
        CreateChannel,
        ChannelList
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

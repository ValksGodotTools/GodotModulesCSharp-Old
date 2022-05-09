namespace GodotModules.Netcode
{
    public struct DataLobby
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte MaxPlayerCount { get; set; }
        public byte HostId { get; set; }
    }
}
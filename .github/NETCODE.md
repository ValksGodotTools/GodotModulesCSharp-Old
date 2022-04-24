#### Threads
The client runs on 2 threads; the Godot thread and the ENet thread. Never run Godot code in the ENet thread and likewise never run ENet code in the Godot thread. If you ever need to communicate between the threads, use the proper `ConcurrentQueue`'s in `ENetClient.cs`.

#### Networking
The netcode utilizes [ENet-CSharp](https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md), a reliable UDP networking library.

Never give the client any authority, the server always has the final say in everything. This should always be thought of when sending new packets.

Packets are sent like this (the way packets are read has a very similar setup)
```cs
namespace GodotModules.Netcode
{
    public class WPacketPlayerData : IWritable
    {
        public uint PlayerId { get; set; }
        public uint PlayerHealth { get; set; }
        public string PlayerName { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PlayerHealth);
            writer.Write(PlayerName);
        }
    }
}

// Since packets are being enqueued to a ConcurrentQueue they can be called from any thread
GameClient.Send(ClientPacketOpcode.PlayerData, new WPacketPlayerData {
    PlayerId = 0,
    PlayerHealth = 100,
    PlayerName = "Steve"
});
```

Consider size of data types when sending them over the network https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm (the smaller the better but keep it practical)

- [x] Post created servers to [NodeJS web server](https://github.com/valkyrienyanko/GodotListServers) / fetch all servers

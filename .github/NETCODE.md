#### Threads
The client runs on 2 threads; the Godot thread and the ENet thread. Never run Godot code in the ENet thread and likewise never run ENet code in the Godot thread. If you ever need to communicate between the threads, use the proper `ConcurrentQueue`'s in `ENetClient.cs`.

#### Networking
The netcode utilizes [ENet-CSharp](https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md), a reliable UDP networking library.

Never give the client any authority, the server always has the final say in everything. This should always be thought of when sending new packets.

Example using LobbyChatMessage packet.
```cs
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyChatMessage : APacketClient // C = Client, A = abstract
    {
        public string Message { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Message);
        }

        public override void Read(PacketReader reader)
        {
            Message = reader.ReadString();
        }

        // the packet handled server-side
        // only use GameServer.Log(...) for debugging
        public override void Handle(ENet.Peer peer)
        {
            GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyChatMessage, new SPacketLobbyChatMessage {
                Id = peer.ID,
                Message = Message
            });
        }
    }
}
```

```cs
// Godot LineEdit node that accepts text input on enter
private async void _on_Chat_Input_text_entered(string text)
{
    ChatInput.Clear();
    if (!string.IsNullOrWhiteSpace(text))
    {
        // Since packets are being enqueued to a ConcurrentQueue they can be called from any thread
        // Make sure to define the LobbyChatMessage opcode for both the client and server in _Opcodes.cs
        await GameClient.Send(ClientPacketOpcode.LobbyChatMessage, new CPacketLobbyChatMessage {
            Message = text.Trim()
        });
    }
}
```

```cs
namespace GodotModules.Netcode 
{
    // only extend from APacketServerPeerId if you're telling other clients about
    // something that changed about a peer, otherwise extend from APacketServer
    public class SPacketLobbyChatMessage : APacketServerPeerId // S = Server
    {
        public string Message { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer); // write the peer id
            writer.Write((string)Message);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader); // read the peer id
            Message = reader.ReadString();
        }

        // the packet handled client-side
        // preferably use GodotModules.Utils.Log(...) for debugging
        public override void Handle()
        {
            // the message is logged to in-game chat
            SceneLobby.Log(Id, Message);
        }
    }
}
```

Have a look at the other packets for more examples.

Consider size of data types when sending them over the network https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm (the smaller the better but keep it practical)

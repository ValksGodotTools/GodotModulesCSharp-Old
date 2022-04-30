using GodotModules.Netcode.Server;
using System.IO;

namespace GodotModules.Netcode
{
    public class CPacketLobby : APacketClient
    {
        public LobbyOpcode LobbyOpcode { get; set; }

        // ChatMessage
        public string Message { get; set; }

        // CountdownChange
        public bool CountdownRunning { get; set; }

        // Join
        public string Username { get; set; }

        // Ready
        public bool Ready { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)LobbyOpcode);

            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    writer.Write(Message);
                    break;
                case LobbyOpcode.LobbyCountdownChange:
                    writer.Write(CountdownRunning);
                    break;
                case LobbyOpcode.LobbyJoin:
                    writer.Write(Username);
                    break;
                case LobbyOpcode.LobbyReady:
                    writer.Write(Ready);
                    break;
            }
        }

        public override void Read(PacketReader reader)
        {
            LobbyOpcode = (LobbyOpcode)reader.ReadByte();

            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    Message = reader.ReadString();
                    break;
                case LobbyOpcode.LobbyCountdownChange:
                    CountdownRunning = reader.ReadBool();
                    break;
                case LobbyOpcode.LobbyJoin:
                    Username = reader.ReadString();
                    break;
                case LobbyOpcode.LobbyReady:
                    Ready = reader.ReadBool();
                    break;
            }
        }

        public override void Handle(ENet.Peer peer)
        {
            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyChatMessage, new SPacketLobbyChatMessage
                    {
                        Id = peer.ID,
                        Message = Message
                    });
                    break;
                case LobbyOpcode.LobbyCountdownChange:
                    GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.LobbyCountdownChange, new SPacketLobbyCountdownChange
                    {
                        //Id = peer.ID,
                        CountdownRunning = CountdownRunning
                    });
                    break;
                case LobbyOpcode.LobbyGameStart:
                    GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyGameStart);
                    break;
                case LobbyOpcode.LobbyJoin:
                    // Check if data.Username is appropriate username
                    // TODO

                    var isHost = false;

                    if (GameServer.Players.Count == 0)
                        isHost = true;

                    // Keep track of joining player server side
                    if (GameServer.Players.ContainsKey(peer.ID))
                    {
                        GameServer.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                        return;
                    }

                    GameServer.Players.Add(peer.ID, new DataPlayer
                    {
                        Username = Username,
                        Ready = false,
                        Host = isHost
                    });

                    // tell joining player their Id and tell them about other players in lobby
                    // also tell them if they are the host or not
                    GameServer.Send(ServerPacketOpcode.LobbyInfo, new SPacketLobbyInfo
                    {
                        Id = peer.ID,
                        IsHost = isHost,
                        Players = GameServer.GetOtherPlayers(peer.ID)
                    }, peer);

                    // tell other players about new player that joined
                    GameServer.SendToOtherPeers(peer.ID, ServerPacketOpcode.LobbyJoin, new SPacketLobbyJoin
                    {
                        Id = peer.ID,
                        Username = Username
                    });
                    break;
                case LobbyOpcode.LobbyReady:
                    var player = GameServer.Players[peer.ID];
                    player.Ready = Ready;

                    GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.LobbyReady, new SPacketLobbyReady
                    {
                        Id = peer.ID,
                        Ready = Ready
                    });
                    break;
            }
        }
    }
}
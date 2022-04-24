using System.Collections.Generic;
using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyInfo : PacketServerPeerId
    {
        public Dictionary<uint, DataPlayer> Players { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
            writer.Write((ushort)Players.Count);
            foreach (var pair in Players)
            {
                writer.Write((ushort)pair.Key); // id

                var player = pair.Value;
                writer.Write((string)player.Username);
            }
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
            var count = reader.ReadUInt16();
            Players = new Dictionary<uint, DataPlayer>();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadUInt16();
                var name = reader.ReadString();

                Players.Add(id, new DataPlayer {
                    Username = name,
                    Ready = false
                });
            }
        }

        public override void Handle()
        {
            ENetClient.PeerId = Id;
            ENetClient.Log($"{GameManager.Options.OnlineUsername} joined lobby with id {Id} also other players in lobby are {Players.Print()}");

            SceneLobby.AddPlayer(Id, GameManager.Options.OnlineUsername);

            foreach (var pair in Players)
                SceneLobby.AddPlayer(pair.Key, pair.Value.Username);

            SceneManager.ChangeScene("Lobby");
        }
    }
}
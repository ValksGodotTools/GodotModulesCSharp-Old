using System.Collections.Generic;
using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyInfo : IPacketServer
    {
        public uint Id { get; set; }
        public Dictionary<uint, string> Players { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((ushort)Players.Count);
            foreach (var player in Players)
            {
                writer.Write((ushort)player.Key);
                writer.Write((string)player.Value);
            }
        }

        public void Read(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            var count = reader.ReadUInt16();
            Players = new Dictionary<uint, string>();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadUInt16();
                var name = reader.ReadString();

                Players.Add(id, name);
            }
        }

        public void Handle()
        {
            ENetClient.Log($"{GameManager.Options.OnlineUsername} joined lobby with id {Id} also other players in lobby are {Players.Print()}");

            SceneLobby.AddPlayer(Id, GameManager.Options.OnlineUsername);

            foreach (var player in Players)
                SceneLobby.AddPlayer(player.Key, player.Value);

            SceneManager.ChangeScene("Lobby");
        }
    }
}
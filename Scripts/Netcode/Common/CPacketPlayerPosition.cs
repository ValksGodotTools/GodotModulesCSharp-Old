using Godot;
using GodotModules.Netcode.Server;
using System;
using System.Linq;

namespace GodotModules.Netcode 
{
    public class CPacketPlayerPosition : APacketClient
    {
        public Vector2 Position { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((float)Math.Round(Position.x, 1));
            writer.Write((float)Math.Round(Position.y, 1));
        }

        public override void Read(PacketReader reader)
        {
            var pos = new Vector2();
            pos.x = reader.ReadFloat();
            pos.y = reader.ReadFloat();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.Players[peer.ID].Position = Position;

            foreach (var pair in GameServer.Players)
            {
                GameServer.Send(ServerPacketOpcode.PlayerPositions, new SPacketPlayerPositions {
                    PlayerPositions = GameServer.Players.Where(x => x.Key != pair.Key).ToDictionary(x => x.Key, x => x.Value.Position)
                }, GameServer.Peers[pair.Key]);
            }
        }
    }
}
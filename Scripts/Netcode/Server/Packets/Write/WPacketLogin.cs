using System.Collections.Generic;
using Common.Netcode;
using Common.Game;

namespace Valk.Modules.Netcode.Server
{
    public class WPacketLogin : IWritable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public Version ServerVersion { get; set; }
        public uint ClientId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((byte)LoginOpcode);

            if (LoginOpcode == LoginResponseOpcode.VersionMismatch) 
            {
                writer.Write((byte)ServerVersion.Major);
                writer.Write((byte)ServerVersion.Minor);
                writer.Write((byte)ServerVersion.Patch);
                return;
            }

            /*// Players
            writer.Write(ENetServer.Players.Count);
            foreach (var pair in ENetServer.Players) 
            {
                var playerId = pair.Key;
                var player = pair.Value;

                writer.Write(playerId);
                writer.Write(player.Username);
            }

            // Channels
            var channels = new Dictionary<uint, Channel>();
            foreach (var pair in ENetServer.Channels)
                if (pair.Value.Users.Contains(ClientId)) // Only send channels that this user is in
                    channels.Add(pair.Key, pair.Value);

            // User IDs
            var userIds = new List<uint>();

            writer.Write(channels.Count);
            foreach (var pair in channels) 
            {
                var channelId = pair.Key;
                var channel = pair.Value;

                writer.Write(channelId);
                writer.Write(channel.Users.Count);
                foreach (var userId in channel.Users)
                    writer.Write(userId);
            }*/
        }
    }
}

using Common.Netcode;
using Godot;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Common.Game;

namespace Valk.Modules.Netcode.Client
{
    public class RPacketLogin : IReadable
    {
        public LoginResponseOpcode LoginOpcode { get; set; }
        public Version Version { get; set; }
        public Dictionary<uint, Player> Players = new Dictionary<uint, Player>();
        public Dictionary<uint, Channel> Channels = new Dictionary<uint, Channel>();

        public void Read(PacketReader reader)
        {
            LoginOpcode = (LoginResponseOpcode)reader.ReadByte();

            if (LoginOpcode == LoginResponseOpcode.VersionMismatch) 
            {
                Version = new Version {
                    Major = reader.ReadByte(),
                    Minor = reader.ReadByte(),
                    Patch = reader.ReadByte()
                };
                return;
            }
        }
    }
}
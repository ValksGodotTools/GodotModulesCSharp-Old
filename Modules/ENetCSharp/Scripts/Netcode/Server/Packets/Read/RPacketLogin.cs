using Common.Netcode;

namespace Valk.Modules.Netcode.Server
{
    public class RPacketLogin : IReadable
    {
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }
        public string JsonWebToken { get; set; }

        public void Read(PacketReader reader)
        {
            VersionMajor = reader.ReadByte();
            VersionMinor = reader.ReadByte();
            VersionPatch = reader.ReadByte();
            JsonWebToken = reader.ReadString();
        }
    }
}

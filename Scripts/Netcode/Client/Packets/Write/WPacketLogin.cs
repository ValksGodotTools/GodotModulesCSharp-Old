using Common.Netcode;

namespace Valk.Modules.Netcode.Client
{
    public class WPacketLogin : IWritable
    {
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }
        public string JsonWebToken { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(VersionMajor);
            writer.Write(VersionMinor);
            writer.Write(VersionPatch);
            writer.Write(JsonWebToken);
        }
    }
}
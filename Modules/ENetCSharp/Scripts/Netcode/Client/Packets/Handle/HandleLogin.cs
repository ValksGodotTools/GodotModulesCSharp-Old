using Common.Netcode;
using ENet;
using System.Linq;
using Godot;

namespace Valk.Modules.Netcode.Client
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ServerPacketOpcode Opcode { get; set; }

        public HandlePacketLogin() => Opcode = ServerPacketOpcode.LoginResponse;

        public override void Handle(PacketReader packetReader)
        {
            var data = new RPacketLogin();
            data.Read(packetReader);

            var opcode = data.LoginOpcode;

            if (opcode == LoginResponseOpcode.InvalidToken)
            {
                //UILogin.UpdateResponse("Invalid token");
                return;
            }

            if (opcode == LoginResponseOpcode.VersionMismatch)
            {
                //var serverVersion = $"{data.Version.Major}.{data.Version.Minor}.{data.Version.Patch}";
                //var clientVersion = $"{GameClient.Version.Major}.{GameClient.Version.Minor}.{GameClient.Version.Patch}";
                //var message = $"Version mismatch. Server ver. {serverVersion} Client ver. {clientVersion}";

                //UILogin.UpdateResponse("Version mismatch");
                return;
            }

            //UILogin.UpdateResponse("Logged in to game server");
            //UILogin.LoadGameScene();
        }
    }
}
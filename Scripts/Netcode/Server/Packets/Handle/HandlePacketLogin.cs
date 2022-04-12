using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime;
using Common.Game;

namespace Valk.Modules.Netcode.Server
{
    public class HandlePacketLogin : HandlePacket
    {
        public override ClientPacketOpcode Opcode { get; set; }

        public HandlePacketLogin() => Opcode = ClientPacketOpcode.Login;

        public override void Handle(Peer peer, PacketReader packetReader)
        {
            var data = new RPacketLogin();
            data.Read(packetReader);

            //var token = ValidateJWT(peer, data);

            //if (token == null)
                //return;

            //if (!ValidateVersion(peer, data, token))
                //return;

            // Check if username exists in database
            //var playerUsername = token.Payload.username;

            // Add player to global channel
            //ENetServer.Channels[(uint)SpecialChannel.Global].Users.Add(peer.ID);

            // These values will be sent to the client
            //WPacketLogin packetData;

            /*var player = PlayerUtils.GetConfig(playerUsername);

            if (player != null)
            {
                // RETURNING PLAYER

                player.Peer = peer;
                //player.AddResourcesGeneratedFromStructures();

                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessReturningPlayer,
                    ClientId = peer.ID
                };

                //player.InGame = true;

                // Add the player to the list of players currently on the server
                ENetServer.Players.Add(peer.ID, player);

                Logger.Log($"Player '{playerUsername}' logged in");
            }
            else
            {
                // NEW PLAYER
                packetData = new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.LoginSuccessNewPlayer,
                    ClientId = peer.ID
                };

                // Add the player to the list of players currently on the server
                // Generate new GUID
                //var guid = new Guid(playerUsername).ToString();
                ENetServer.Players.Add(peer.ID, new ServerPlayer(peer, playerUsername));

                // Add player to database
                var players = Database.Db.GetCollection<PlayerModel>("players");
                var query = await players.FindAsync(x => x.Name == playerUsername);
                var results = query.ToList();

                if (results.Count == 0) 
                {
                    await players.InsertOneAsync(new PlayerModel {
                        Name = playerUsername
                    });
                }

                Logger.Log($"User '{playerUsername}' logged in for the first time");
            }

            ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, packetData, peer));*/
        }

        /*private static JsonWebToken ValidateJWT(Peer peer, RPacketLogin data) 
        {
            // Did the client provide a valid JWT?
            var token = new JsonWebToken(data.JsonWebToken);
            if (token.IsValid.Error != JsonWebToken.TokenValidateError.Ok)
            {
                ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.InvalidToken
                }, peer));
                return null;
            }
            return token;
        }

        private static bool ValidateVersion(Peer peer, RPacketLogin data, JsonWebToken token) 
        {
            // Does the client version match the server version?
            if (data.VersionMajor != ENetServer.Version.Major || data.VersionMinor != ENetServer.Version.Minor || data.VersionPatch != ENetServer.Version.Patch)
            {
                var clientVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                var serverVersion = $"{ENetServer.Version.Major}.{ENetServer.Version.Minor}.{ENetServer.Version.Patch}";

                Logger.Log($"Player '{token.Payload.username}' tried to log in but failed because they are running on version " +
                    $"'{clientVersion}' but the server is on version '{serverVersion}'");

                ENetServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LoginResponse, new WPacketLogin
                {
                    LoginOpcode = LoginResponseOpcode.VersionMismatch,
                    ServerVersion = ENetServer.Version
                }, peer));

                return false;
            }

            return true;
        }*/
    }
}

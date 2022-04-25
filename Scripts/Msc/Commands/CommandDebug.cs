using GodotModules.Netcode.Client;
using System.Collections.Generic;

namespace GodotModules 
{
    public class CommandDebug : Command 
    {
        public CommandDebug() => Aliases = new string[] { "d" };

        public override void Run(string[] args)
        {
            var info = new string[] {
                $"Username: {GameManager.Options.OnlineUsername}",
                $"Id: {ENetClient.PeerId}"
            };

            Utils.Log(info.Print());

            var players = SceneLobby.GetPlayers();

            if (players == null)
                return;

            foreach (var pair in players)
            {
                var id = pair.Key;
                
                var player = pair.Value;

                var username = player.Username;
                var ready = player.Ready;

                Utils.Log("Players in Lobby: " + players.Count);
                Utils.Log($"{id} {username} {ready}");
            }
        }
    }
}
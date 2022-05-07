using System;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules
{
    public class CommandDebug : Command
    {
        public CommandDebug() => Aliases = new string[] { "x" };
        public static float TEST_VALUE = 0.1f;

        public override void Run(string[] args)
        {
            // debug command
            // do debug stuff here
            if (args.Length == 0)
                return;

            if (args[0] == "a")
                if (NetworkManager.GameServer != null)
                    if (NetworkManager.GameServer.IsRunning)
                    {
                        Logger.LogDebug(NetworkManager.GameServer.Players.Print());
                    }
        }
    }
}
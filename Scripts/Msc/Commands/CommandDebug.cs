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
            /*if (args.Length == 0)
                return;

            if (!int.TryParse(args[0], out int result))
                return;

            if (NetworkManager.GameServer.Running)
            {
                NetworkManager.GameServer.EmitClientTransforms.Stop();
                NetworkManager.GameServer.EmitClientTransforms.SetDelay(result);
                NetworkManager.GameServer.EmitClientTransforms.Start();
            }

            TEST_VALUE = result;

            Utils.Log("Set test value to " + result);*/
        }
    }
}
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System.Collections.Generic;
using System.Linq;
using Game;

namespace GodotModules
{
    public class CommandDebug : Command
    {
        public CommandDebug() => Aliases = new string[] { "x" };

        public static int TestValue1 = 200;

        public override void Run(string[] args)
        {
            // debug command
            // do debug stuff here
            if (args.Length == 0)
                return;

            if (!int.TryParse(args[0], out int result))
                return;

            TestValue1 = result;
            Utils.Log("Set test value to " + result);

            if (GameServer.Running)
            {
                GameServer.EmitClientPositions.Stop();
                GameServer.EmitClientPositions.Interval = TestValue1;
                GameServer.EmitClientPositions.Start();
            }

            if (GameClient.Running)
            {
                ClientPlayer.Timer.Stop();
                ClientPlayer.Timer.WaitTime = CommandDebug.TestValue1 / 1000f;
                ClientPlayer.Timer.Start();
            }

        }
    }
}
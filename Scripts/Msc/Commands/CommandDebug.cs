using System;

namespace GodotModules
{
    public class CommandDebug : Command
    {
        public CommandDebug() => Aliases = new string[] { "x" };

        public static int SendReceiveDataInterval = 150;

        public override void Run(string[] args)
        {
            // debug command
            // do debug stuff here

            GameManager.SpawnPopupError(new Exception("lol beans"));

            /*if (args.Length == 0)
                return;

            if (!int.TryParse(args[0], out int result))
                return;

            SendReceiveDataInterval = result;
            Utils.Log("Set test value to " + result);

            if (GameServer.Running)
            {
                GameServer.EmitClientPositions.Stop();
                GameServer.EmitClientPositions.Interval = SendReceiveDataInterval;
                GameServer.EmitClientPositions.Start();
            }

            if (GameClient.Running)
            {
                ClientPlayer.Timer.Stop();
                ClientPlayer.Timer.Start(CommandDebug.SendReceiveDataInterval);
            }*/
        }
    }
}
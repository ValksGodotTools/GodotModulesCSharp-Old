using System;
using GodotModules.Netcode.Client;

namespace GodotModules
{
    public class CommandDebug : Command
    {
        public CommandDebug() => Aliases = new string[] { "x" };

        public static int SendReceiveDataInterval = 150;
        private static List<ENetClient> dummyClients = new List<ENetClient>();
        public static DateTime timeSent;

        public override async void Run(string[] args)
        {
            // debug command
            // do debug stuff here
            if (args.Length == 0)
                return;

            if (args[0] == "add")
            {
                var dummyClient = new ENetClient();
                dummyClients.Add(dummyClient);
                dummyClient.Start("127.0.0.1", 7777);
                timeSent = DateTime.Now;

                while (!dummyClient.IsConnected)
                    await System.Threading.Tasks.Task.Delay(100);

                await dummyClient.Send(Netcode.ClientPacketOpcode.Ping);
            }

            if (args[0] == "rem") 
            {
                dummyClients.ForEach(async x => await x.Stop());
            }

            /*if (!int.TryParse(args[0], out int result))
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
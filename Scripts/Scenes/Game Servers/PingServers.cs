using GodotModules.Netcode.Client;
using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    public class PingServers
    {
        public static DateTime PingSent { get; set; }
        public static bool PingReceived { get; set; }
        public static int PingMs { get; set; }
        public static CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        private static ENetClient DummyClient { get; set; }

        public static async Task PingServer()
        {
            DummyClient = new ENetClient();
            DummyClient.Start("127.0.0.1", 7777);

            while (!DummyClient.IsConnected) 
            {
                if (CancelTokenSource.IsCancellationRequested)
                {
                    DummyClient.Stop();
                    return;
                }

                await Task.Delay(100);
            }

            await DummyClient.Send(Netcode.ClientPacketOpcode.Ping);

            PingSent = DateTime.Now;

            while (!PingReceived) 
            {
                if (CancelTokenSource.IsCancellationRequested) 
                {
                    DummyClient.Stop();
                    return;
                }

                await Task.Delay(1);
            }

            GD.Print($"Ping received ({PingMs}ms)");
            PingReceived = false;

            DummyClient.Stop();
        }
    }
}
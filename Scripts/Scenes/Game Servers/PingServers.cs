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
        public static ENetClient DummyClient { get; set; }

        public static async Task PingServer()
        {
            DummyClient = new ENetClient();
            DummyClient.Start("127.0.0.1", 7777);

            while (!DummyClient.IsConnected) 
                await Task.Delay(100, CancelTokenSource.Token);

            await DummyClient.Send(Netcode.ClientPacketOpcode.Ping);

            PingSent = DateTime.Now;

            while (!PingReceived) 
                await Task.Delay(1, CancelTokenSource.Token);

            GD.Print($"Ping received ({PingMs}ms)");
            PingReceived = false;
        }
    }
}
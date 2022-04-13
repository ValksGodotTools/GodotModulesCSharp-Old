using Godot;
using System;
using System.Threading.Tasks;
using Valk.Modules;
using Valk.Modules.Netcode.Server;

namespace Valk.Modules.Netcode
{
    public class UIServerManager : Node
    {
        [Export] public readonly NodePath NodePathIp;
        [Export] public readonly NodePath NodePathLogger;

        private static LineEdit InputIp { get; set; }
        private static RichTextLabel Logger { get; set; }

        public override void _Ready()
        {
            InputIp = GetNode<LineEdit>(NodePathIp);
            Logger = GetNode<RichTextLabel>(NodePathLogger);
        }

        public static void Log(string text) => Logger.AddText($"{text}\n");
        
        private void _on_Start_pressed() => GameManager.GameServer.Start();
        private void _on_Stop_pressed() => GameManager.GameServer.Stop();
        private void _on_Restart_pressed() => GameManager.GameServer.Restart();
        private void _on_Connect_pressed() => GameManager.GameClient.Connect(InputIp.Text, 25565);
        private void _on_Disconnect_pressed() => GameManager.GameClient.Disconnect();
    }

}

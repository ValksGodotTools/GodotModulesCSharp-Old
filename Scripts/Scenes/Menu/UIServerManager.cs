using Godot;

namespace GodotModules.Netcode
{
    public class UIServerManager : Node
    {
        [Export] public readonly NodePath NodePathIp;
        [Export] public readonly NodePath NodePathLogger;

        private LineEdit InputIp { get; set; }
        private RichTextLabel Logger { get; set; }
        public static UIServerManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            InputIp = GetNode<LineEdit>(NodePathIp);
            Logger = GetNode<RichTextLabel>(NodePathLogger);
        }

        public void Log(string text) => Logger.AddText($"{text}\n");

        private void _on_Start_pressed() => GameManager.GameServer.Start();

        private void _on_Stop_pressed() => GameManager.GameServer.Stop();

        private void _on_Restart_pressed() => GameManager.GameServer.Restart();

        private void _on_Connect_pressed() => GameManager.GameClient.Connect(InputIp.Text, 25565);

        private void _on_Disconnect_pressed() => GameManager.GameClient.Disconnect();
    }
}
using Godot;

namespace GodotModules
{
    public class UIPopupDirectConnect : WindowDialog
    {
        [Export] public readonly NodePath NodePathIp;

        private LineEdit Ip { get; set; }

        public override void _Ready()
        {
            Ip = GetNode<LineEdit>(NodePathIp);
        }

        private string previousText = "";

        private void _on_Ip_text_changed(string text) => text.Validate(ref previousText, Ip, () => text.IsMatch("^[A-Za-z0-9:.]+$"));

        private async void _on_Ok_pressed()
        {
            var input = Ip.Text;

            ushort port = 7777; // default port
            var ip = "127.0.0.1";

            var colonIndex = input.IndexOf(':');

            if (colonIndex == -1)
                return;

            if (ushort.TryParse(input.Substring(colonIndex + 1), out ushort result))
            {
                port = result;
                ip = input.Substring(0, colonIndex);
            }

            Hide();

            await SceneGameServers.JoinServer(new LobbyListing {
                Ip = ip,
                Port = port,
                Name = "Unknown Name"
            });
        }

        private void _on_PopupDirectConnect_popup_hide()
        {
            QueueFree();
        }
    }
}
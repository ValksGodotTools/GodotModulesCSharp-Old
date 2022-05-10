using Godot;
using GodotModules.Settings;

namespace Game
{
    // DEMO
    public class SceneMenu : AScene
    {
        [Export] public readonly NodePath NodePathSetupOnlineUsernamePopup;

        private UISetupOnlineProfile PopupOnlineUsername;

        public override void _Ready()
        {
            PopupOnlineUsername = GetNode<UISetupOnlineProfile>(NodePathSetupOnlineUsernamePopup);

            MusicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            MusicManager.PlayTrack("Menu");
        }

        private async void _on_Play_pressed() => await SceneManager.ChangeScene("Game");

        private async void _on_Multiplayer_pressed()
        {
            var onlineUsername = GM.Options.OnlineUsername;

            if (string.IsNullOrWhiteSpace(onlineUsername))
            {
                PopupOnlineUsername.InputUsername.Clear();
                PopupOnlineUsername.LabelFeedback.Text = "";
                PopupOnlineUsername.Popup_();
                return;
            }

            await SceneManager.ChangeScene("GameServers");
        }

        private void _on_Exit_pressed() => GM.Exit();

        private async void _on_Set_Online_Username_Popup_confirmed()
        {
            var username = PopupOnlineUsername.InputUsername.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                PopupOnlineUsername.LabelFeedback.Text = "Invalid username";
                return;
            }

            UIOptions.Instance.InputUsername.Text = username;
            GM.Options.OnlineUsername = username;
            PopupOnlineUsername.Hide();

            await SceneManager.ChangeScene("GameServers");
        }

        public override void Cleanup()
        {
            
        }
    }
}
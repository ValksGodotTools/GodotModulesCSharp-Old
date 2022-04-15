using Godot;
using GodotModules;
using GodotModules.Netcode;
using GodotModules.Settings;

namespace Game
{
    // DEMO
    public class UIMenu : Node
    {
        [Export] public readonly NodePath NodePathSetupOnlineUsernamePopup;

        private UISetupOnlineProfile PopupOnlineUsername;

        public override void _Ready()
        {
            PopupOnlineUsername = GetNode<UISetupOnlineProfile>(NodePathSetupOnlineUsernamePopup);

            MusicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            MusicManager.PlayTrack("Menu");
        }

        private void _on_Play_pressed() => GetTree().ChangeScene("res://Scenes/Game.tscn");

        private void _on_Multiplayer_pressed()
        {
            var onlineUsername = UIOptions.Options.OnlineUsername;

            if (string.IsNullOrWhiteSpace(onlineUsername))
            {
                PopupOnlineUsername.InputUsername.Clear();
                PopupOnlineUsername.LabelFeedback.Text = "";
                PopupOnlineUsername.Popup_();
                return;
            }

            GetTree().ChangeScene("res://Scenes/GameServers.tscn");
        }

        private void _on_Exit_pressed() => GameManager.Exit();

        private void _on_Set_Online_Username_Popup_confirmed()
        {
            var username = PopupOnlineUsername.InputUsername.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                PopupOnlineUsername.LabelFeedback.Text = "Invalid username";
                return;
            }

            UIOptions.InputUsername.Text = username;
            UIOptions.Options.OnlineUsername = username;
            PopupOnlineUsername.Hide();

            GetTree().ChangeScene("res://Scenes/GameServers.tscn");
        }
    }
}
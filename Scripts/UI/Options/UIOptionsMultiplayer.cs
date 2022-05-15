using Godot;
using System.Text.RegularExpressions;

namespace GodotModules
{
    public class UIOptionsMultiplayer : Control
    {
        [Export] public readonly NodePath NodePathOnlineUsername;
        [Export] public readonly NodePath NodePathWebServerAddress;
        [Export] public readonly NodePath NodePathBtnCheckConnection;
        [Export] public readonly NodePath NodePathWebServerStatus;
        private LineEdit _onlineUsername;
        private LineEdit _webServerAddress;
        private Button _btnCheckConnection;
        private Label _webServerStatus;

        private OptionsManager _optionsManager;
        private WebManager _webManager;
        private SceneManager _sceneManager;
        private TokenManager _tokenManager;

        public void PreInit(OptionsManager optionsManager, WebManager webManager, SceneManager sceneManager, TokenManager tokenManager)
        {
            _optionsManager = optionsManager;
            _webManager = webManager;
            _sceneManager = sceneManager;
            _tokenManager = tokenManager;
        }

        public override void _Ready()
        {
            _onlineUsername = GetNode<LineEdit>(NodePathOnlineUsername);
            _webServerAddress = GetNode<LineEdit>(NodePathWebServerAddress);
            _btnCheckConnection = GetNode<Button>(NodePathBtnCheckConnection);
            _webServerStatus = GetNode<Label>(NodePathWebServerStatus);

            _onlineUsername.Text = _optionsManager.Options.OnlineUsername;
            _webServerAddress.Text = _optionsManager.Options.WebServerAddress;
            SetWebServerStatus(_webManager.ConnectionAlive);
        }

        private void _on_OnlineUsername_text_changed(string newText)
        {
            _optionsManager.Options.OnlineUsername = _onlineUsername.Filter((text) => Regex.IsMatch(text, "^[a-z]+$"));
        }

        private void _on_WebServerAddress_text_changed(string newText)
        {
            _webManager.Ip = newText;
            _optionsManager.Options.WebServerAddress = newText;
        }

        private async void _on_Check_Connection_pressed()
        {
            _btnCheckConnection.Disabled = true;
            _webServerStatus.Text = "Checking...";

            await _webManager.CheckConnectionAsync(_tokenManager.Create("check_connection"));

            if (!_tokenManager.Cancelled("check_connection"))
            {
                _btnCheckConnection.Disabled = false;
                SetWebServerStatus(_webManager.ConnectionAlive);
            }
        }

        private void SetWebServerStatus(bool online) => _webServerStatus.Text = $"Connection to web server {(online ? "successful" : "failed")}";
    }
}

namespace GodotModules
{
    public class UIOptionsMultiplayer : Control
    {
        [Export] protected readonly NodePath NodePathOnlineUsername;
        [Export] protected readonly NodePath NodePathWebServerAddress;
        [Export] protected readonly NodePath NodePathBtnCheckConnection;
        [Export] protected readonly NodePath NodePathWebServerStatus;
        private LineEdit _onlineUsername;
        private LineEdit _webServerAddress;
        private Button _btnCheckConnection;
        private Label _webServerStatus;

        private Options _optionsManager;
        private WebManager _webManager;
        private Tokens _tokenManager;

        public void PreInit(Options optionsManager, WebManager webManager, Tokens tokenManager)
        {
            _optionsManager = optionsManager;
            _webManager = webManager;
            _tokenManager = tokenManager;
        }

        public override void _Ready()
        {
            _onlineUsername = GetNode<LineEdit>(NodePathOnlineUsername);
            _webServerAddress = GetNode<LineEdit>(NodePathWebServerAddress);
            _btnCheckConnection = GetNode<Button>(NodePathBtnCheckConnection);
            _webServerStatus = GetNode<Label>(NodePathWebServerStatus);

            _onlineUsername.Text = _optionsManager.Data.OnlineUsername;
            _webServerAddress.Text = _optionsManager.Data.WebServerAddress;
            SetWebServerStatus(_webManager.ConnectionAlive);
        }

        private void _on_OnlineUsername_text_changed(string newText)
        {
            _optionsManager.Data.OnlineUsername = _onlineUsername.Filter((text) => Regex.IsMatch(text, "^[a-z]+$"));
        }

        private void _on_WebServerAddress_text_changed(string newText)
        {
            _webManager.Ip = newText;
            _optionsManager.Data.WebServerAddress = newText;
        }

        private async void _on_Check_Connection_pressed()
        {
            _btnCheckConnection.Disabled = true;
            _webServerStatus.Text = "Checking...";

            await _webManager.CheckConnectionAsync();

            if (!_tokenManager.Cancelled("check_connection"))
            {
                _btnCheckConnection.Disabled = false;
                SetWebServerStatus(_webManager.ConnectionAlive);
            }
        }

        private void SetWebServerStatus(bool online) => _webServerStatus.Text = $"Connection to web server {(online ? "successful" : "failed")}";
    }
}

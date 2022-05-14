using Godot;
using System.Text.RegularExpressions;

namespace GodotModules
{
    public class UIOptionsMultiplayer : Control
    {
        [Export] public readonly NodePath NodePathOnlineUsername;
        [Export] public readonly NodePath NodePathWebServerAddress;
        private LineEdit _onlineUsername;
        private LineEdit _webServerAddress;
        private OptionsManager _optionsManager;

        public void PreInit(OptionsManager optionsManager)
        {
            _optionsManager = optionsManager;
            _onlineUsername.Text = _optionsManager.Options.OnlineUsername;
            _webServerAddress.Text = _optionsManager.Options.WebServerAddress;
        }

        public override void _Ready()
        {
            _onlineUsername = GetNode<LineEdit>(NodePathOnlineUsername);
            _webServerAddress = GetNode<LineEdit>(NodePathWebServerAddress);
        }

        private void _on_OnlineUsername_text_changed(string newText)
        {
            _optionsManager.Options.OnlineUsername = _onlineUsername.Filter((text) => Regex.IsMatch(text, "^[A-Za-z]+$"));
        }

        private void _on_WebServerAddress_text_changed(string newText)
        {
            _optionsManager.Options.WebServerAddress = newText;
        }
    }
}

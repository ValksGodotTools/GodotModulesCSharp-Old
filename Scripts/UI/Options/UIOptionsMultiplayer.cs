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

        public override void _Ready()
        {
            _onlineUsername = GetNode<LineEdit>(NodePathOnlineUsername);
            _webServerAddress = GetNode<LineEdit>(NodePathWebServerAddress);
        }

        private string prevTextOnlineUsername = "";

        private void _on_OnlineUsername_text_changed(string newText)
        {
            _onlineUsername.Filter((text) => Regex.IsMatch(text, "^[A-Za-z]+$"));
        }

        private void _on_WebServerAddress_text_changed(string newText)
        {

        }
    }
}

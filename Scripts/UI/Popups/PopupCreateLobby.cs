using Godot;

namespace GodotModules 
{
    public class PopupCreateLobby : WindowDialog
    {
        [Export] public readonly NodePath NodePathName;
        [Export] public readonly NodePath NodePathDescription;
        [Export] public readonly NodePath NodePathPort;
        [Export] public readonly NodePath NodePathMaxPlayers;
        [Export] public readonly NodePath NodePathPassword;

        private LineEdit _name;
        private TextEdit _description;
        private LineEdit _port;
        private LineEdit _maxPlayers;
        private LineEdit _password;

        public override void _Ready()
        {
            _name = GetNode<LineEdit>(NodePathName);
            _description = GetNode<TextEdit>(NodePathDescription);
            _port = GetNode<LineEdit>(NodePathPort);
            _maxPlayers = GetNode<LineEdit>(NodePathMaxPlayers);
            _password = GetNode<LineEdit>(NodePathPassword);
        }
    }
}

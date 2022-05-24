namespace GodotModules 
{
    public class UILobby : Node
    {
        [Export] protected readonly NodePath NodePathLobbyName;
        [Export] protected readonly NodePath NodePathLobbyPlayerCount;
        [Export] protected readonly NodePath NodePathPlayerList;
        [Export] protected readonly NodePath NodePathChatLogs;

        private Label _lobbyName;
        private Label _lobbyPlayerCount;
        private Control _playerList;
        private RichTextLabel _chatLogs;

        public override void _Ready()
        {
            _lobbyName = GetNode<Label>(NodePathLobbyName);
            _lobbyPlayerCount = GetNode<Label>(NodePathLobbyPlayerCount);
            _playerList = GetNode<Control>(NodePathPlayerList);
            _chatLogs = GetNode<RichTextLabel>(NodePathChatLogs);
        }

        private void _on_Chat_Input_text_entered(string text)
        {

        }
    }
}

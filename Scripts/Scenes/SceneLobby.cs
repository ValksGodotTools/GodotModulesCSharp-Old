namespace GodotModules 
{
    public class SceneLobby : AScene
    {
        [Export] protected readonly NodePath NodePathLobbyName;
        [Export] protected readonly NodePath NodePathLobbyPlayerCount;
        [Export] protected readonly NodePath NodePathPlayerList;
        [Export] protected readonly NodePath NodePathChatLogs;

        private Label _lobbyName;
        private Label _lobbyPlayerCount;
        private Control _playerList;
        private RichTextLabel _chatLogs;

        private Managers _managers;

        public override void PreInitManagers(Managers managers)
        {
            _managers = managers;
        }

        public override void _Ready()
        {
            _lobbyName = GetNode<Label>(NodePathLobbyName);
            _lobbyPlayerCount = GetNode<Label>(NodePathLobbyPlayerCount);
            _playerList = GetNode<Control>(NodePathPlayerList);
            _chatLogs = GetNode<RichTextLabel>(NodePathChatLogs);

            Notifications.AddListener(this, Event.OnKeyboardInput, (sender, args) => {
                _managers.ManagerScene.HandleEscape(async () => {
                    _managers.Tokens.Cancel("client_running");
                    _managers.Tokens.Cancel("server_running");
                    await _managers.ManagerScene.ChangeScene(GameScene.GameServers);
                });
            });
        }

        private void _on_Chat_Input_text_entered(string text)
        {

        }
    }
}

namespace GodotModules
{
    public class SceneGameServers : AScene
    {
        [Export] protected readonly NodePath NodePathLobbyList;

        private List<UILobbyListing> _lobbyListings = new();
        private Control _lobbyList;
        private Managers _managers;
        private bool _connectingToLobby;

        public override void PreInitManagers(Managers managers)
        {
            _managers = managers;
        }

        public override void _Ready()
        {
            _lobbyList = GetNode<Control>(NodePathLobbyList);

            var colorSwitch = false;

            for (int i = 0; i < 20; i++)
            {
                var lobby = Prefabs.UILobbyListing.Instance<UILobbyListing>();
                _lobbyList.AddChild(lobby);
                lobby.SetLobbyName(GD.Randf() > 0.5f ? "Foo's Lobby" : "Bar's Lobby");
                lobby.SetPlayers((int)GD.RandRange(0, 10), (int)GD.RandRange(11, 100));
                lobby.SetPing((int)GD.RandRange(0, 500));
                lobby.SetDescription("Some description");
                lobby.SetDark(colorSwitch);
                _lobbyListings.Add(lobby);

                colorSwitch = !colorSwitch;
            }

            _sortPingPressed = true;
            Sort(x => x.Ping, _sortPingPressed);

            Notifications.AddListener(this, Event.OnKeyboardInput, (sender, args) => {
                _managers.ManagerScene.HandleEscape(async () => {
                    _managers.Tokens.Cancel("client_running");
				    await _managers.ManagerScene.ChangeScene(GameScene.Menu);
                });
            });
        }

        private void _on_Create_Lobby_pressed() 
        {
            if (_connectingToLobby)
                return;
            
            _managers.Popups.SpawnCreateLobby();
        }

        private void _on_Direct_Connect_pressed()
        {
            if (_connectingToLobby)
                return;

            _managers.Popups.SpawnLineEdit
            (
                (lineEdit) =>
                {
                    // no realtime filter
                },
                async (text) =>
                {
                    ushort port = 25565; // default port
                    var ip = "127.0.0.1"; // default ip

                    // try to extract ip and port from text
                    var colonIndex = text.IndexOf(':');

                    if (colonIndex == -1)
                        return;

                    if (ushort.TryParse(text.Substring(colonIndex + 1), out ushort result))
                    {
                        port = result;
                        ip = text.Substring(0, colonIndex);
                    }

                    _connectingToLobby = true;

                    var cts = _managers.Tokens.Create("client_running");

                    _managers.Net.StartClient(ip, port, cts);

                    try
                    {
                        while (!_managers.Net.Client.IsConnected)
                            await Task.Delay(1, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // There are many reasons why "client_running" task could be cancelled, here are a few
                        // 1. user backed out of game servers to main menu or backed out of lobby to game servers
                        // 2. client disconnected or timed out
                        _connectingToLobby = false;
                        return;
                    }

                    // Client connected
                    await _managers.ManagerScene.ChangeScene(GameScene.Lobby);
                    _connectingToLobby = false;
                },
                "Direct Connect", 100, "127.0.0.1:25565"
            );
        }

        private void _on_Refresh_pressed()
        {

        }

        private bool _sortNamePressed;
        private bool _sortPlayersPressed;
        private bool _sortPingPressed;
        private bool _sortDescriptionPressed;

        private void _on_SortName_pressed()
        {
            _sortNamePressed = !_sortNamePressed;
            Sort(x => x.LobbyName, _sortNamePressed);
        }

        private void _on_SortPlayers_pressed()
        {
            _sortPlayersPressed = !_sortPlayersPressed;
            Sort(x => x.CurPlayers, _sortPlayersPressed);
        }

        private void _on_SortPing_pressed()
        {
            _sortPingPressed = !_sortPingPressed;
            Sort(x => x.Ping, _sortPingPressed);
        }

        private void _on_SortDescription_pressed()
        {
            _sortDescriptionPressed = !_sortDescriptionPressed;
            Sort(x => x.Description, _sortDescriptionPressed);
        }

        private void Sort<T>(Func<UILobbyListing, T> keySelector, bool ascending = true)
        {
            foreach (UILobbyListing child in _lobbyList.GetChildren())
                _lobbyList.RemoveChild(child);

            var colorSwitch = false;

            foreach (var lobbyListing in ascending ? _lobbyListings.OrderBy(keySelector) : _lobbyListings.OrderByDescending(keySelector))
            {
                _lobbyList.AddChild(lobbyListing);
                lobbyListing.SetDark(colorSwitch);
                colorSwitch = !colorSwitch;
            }
        }
    }
}

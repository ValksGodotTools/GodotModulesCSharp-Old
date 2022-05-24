

namespace GodotModules
{
    public class SceneGameServers : AScene
    {
        [Export] protected readonly NodePath NodePathLobbyList;

        private List<UILobbyListing> _lobbyListings = new();
        private Control _lobbyList;

        public override void PreInitManagers(Managers managers)
        {
            
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
        }

        private void _on_Create_Lobby_pressed()
        {

        }

        private void _on_Direct_Connect_pressed()
        {

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

using Godot;
using GodotModules.Netcode.Client;

namespace GodotModules
{
    public class UIGameServersNavBtns : Node
    {
        [Export] public readonly NodePath NodePathRefresh;
        public static Button BtnRefresh { get; set; }
        private SceneGameServers SceneGameServersScript { get; set; }

        public override void _Ready()
        {
            BtnRefresh = GetNode<Button>(NodePathRefresh);
            SceneGameServersScript = SceneManager.GetActiveSceneScript<SceneGameServers>();
        }

        private async void _on_Join_Lobby_pressed()
        {
            var selected = SceneGameServersScript.SelectedLobbyInstance;

            if (selected == null)
                return;

            await selected.Join();
        }

        private void _on_Create_Lobby_pressed()
        {
            var popup = SceneGameServersScript.ServerCreationPopup;

            if (popup.Visible)
                return;

            popup.ClearFields();
            popup.PopupCentered();
        }

        private async void _on_Refresh_pressed()
        {
            if (SceneGameServersScript.GettingServers)
                return;

            SceneGameServersScript.ClearServers();

            if (NetworkManager.WebClient.ConnectionAlive)
                await SceneGameServersScript.ListServers();
        }

        private void _on_Direct_Connect_pressed()
        {
            var popup = Prefabs.PopupDirectConnect.Instance<UIPopupDirectConnect>();
            GetTree().Root.AddChild(popup);
            popup.PopupCentered();
        }
    }
}
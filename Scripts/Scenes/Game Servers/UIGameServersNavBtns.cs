using Godot;
using GodotModules.Netcode;

namespace GodotModules
{
    public class UIGameServersNavBtns : Node
    {
        [Export] public readonly NodePath NodePathRefresh;
        public static Button BtnRefresh { get; set; }

        public override void _Ready()
        {
            BtnRefresh = GetNode<Button>(NodePathRefresh);
        }

        private async void _on_Join_Lobby_pressed()
        {
            var selected = SceneGameServers.SelectedLobbyInstance;

            if (selected == null)
                return;

            await selected.Join();
        }

        private void _on_Create_Lobby_pressed()
        {
            var popup = SceneGameServers.Instance.ServerCreationPopup;

            if (popup.Visible)
                return;

            popup.ClearFields();
            popup.PopupCentered();
        }

        private async void _on_Refresh_pressed()
        {
            if (SceneGameServers.GettingServers)
                return;

            SceneGameServers.Instance.ClearServers();
            await SceneGameServers.Instance.ListServers();
        }

        private void _on_Direct_Connect_pressed()
        {
            var popup = Prefabs.PopupDirectConnect.Instance<UIPopupDirectConnect>();
            GetTree().Root.AddChild(popup);
            popup.PopupCentered();
        }
    }
}
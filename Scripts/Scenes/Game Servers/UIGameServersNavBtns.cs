using Godot;

namespace GodotModules.Netcode
{
    public class UIGameServersNavBtns : Node
    {
        private void _on_Join_Lobby_pressed()
        {
            var selected = SceneGameServers.SelectedLobbyInstance;

            if (selected == null)
                return;

            selected.Join();
        }

        private void _on_Create_Lobby_pressed()
        {
            var popup = SceneGameServers.Instance.ServerCreationPopup;

            if (popup.Visible)
                return;

            popup.ClearFields();
            popup.ClearFeedback();
            popup.PopupCentered();
        }

        private async void _on_Refresh_pressed()
        {
            if (!GameManager.WebClient.TaskGetServers.IsCompleted)
                return;

            SceneGameServers.Instance.ClearServers();
            await SceneGameServers.Instance.ListServers();
        }
    }
}
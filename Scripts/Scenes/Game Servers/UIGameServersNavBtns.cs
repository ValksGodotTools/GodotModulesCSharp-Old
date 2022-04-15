using Godot;

namespace GodotModules.Netcode
{
    public class UIGameServersNavBtns : Node
    {
        private void _on_Join_Lobby_pressed()
        {
            var selected = UILobbyListing.Instance.CurrentListingFocused;

            if (selected == null)
                return;

            selected.Join();
        }

        private void _on_Create_Lobby_pressed()
        {
            var popup = UIGameServers.Instance.ServerCreationPopup;

            if (popup.Visible)
                return;

            popup.ClearFields();
            popup.ClearFeedback();
            popup.PopupCentered();
        }

        private void _on_Refresh_pressed()
        {
            if (!GameManager.WebClient.TaskGetServers.IsCompleted)
                return;

            UIGameServers.Instance.ClearServers();
            UIGameServers.Instance.ListServers();
        }
    }
}
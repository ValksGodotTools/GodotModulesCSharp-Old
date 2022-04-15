using Godot;

namespace GodotModules.Netcode
{
    public class UIGameServersNavBtns : Node
    {
        private void _on_Join_Lobby_pressed()
        {
            var selected = UILobbyListing.CurrentListingFocused;

            if (selected == null)
                return;

            selected.Join();
        }

        private void _on_Create_Lobby_pressed()
        {
            if (UIGameServers.Instance.ServerCreationPopup.Visible)
                return;

            UIGameServers.Instance.ServerCreationPopup.ClearFields();
            UIGameServers.Instance.ServerCreationPopup.ClearFeedback();
            UIGameServers.Instance.ServerCreationPopup.PopupCentered();
        }

        private void _on_Refresh_pressed()
        {
        }
    }
}
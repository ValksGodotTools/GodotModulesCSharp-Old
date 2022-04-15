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
            if (UIGameServers.ServerCreationPopup.Visible)
                return;

            UIGameServers.ServerCreationPopup.ClearFields();
            UIGameServers.ServerCreationPopup.ClearFeedback();
            UIGameServers.ServerCreationPopup.PopupCentered();
        }

        private void _on_Refresh_pressed()
        {
        }
    }
}
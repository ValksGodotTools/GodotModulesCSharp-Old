using Godot;
using System;

namespace GodotModules
{
    public class PopupManager : Control
    {
        public void SpawnPopupMessage(string message, string title = "") 
        {
            var popupMessage = Prefabs.UIPopupMessage.Instance<UIPopupMessage>();
            popupMessage.PreInit(message, title);
            AddChild(popupMessage);
            popupMessage.PopupCentered();
        }

        public void SpawnPopupError(Exception exception, string title = "") 
        {
            var popupError = Prefabs.UIPopupError.Instance<UIPopupError>();
            popupError.PreInit(exception, title);
            AddChild(popupError);
            popupError.PopupCentered();
        }
    }
}

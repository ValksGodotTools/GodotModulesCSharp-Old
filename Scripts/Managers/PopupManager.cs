using Godot;

namespace GodotModules
{
    public class PopupManager : Node
    {
        private Queue<WindowDialog> _popups = new();

        public void SpawnPopupMessage(string message, string title = "")
        {
            var popupMessage = Prefabs.UIPopupMessage.Instance<PopupMessage>();
            popupMessage.PreInit(this, message, title);
            
            Spawn(popupMessage);
        }

        public void SpawnPopupError(Exception exception, string title = "")
        {
            var popupError = Prefabs.UIPopupError.Instance<PopupError>();
            popupError.PreInit(this, exception, title);
            
            Spawn(popupError);
        }

        public void SpawnNextPopup() 
        {
            _popups.Dequeue();
            if (_popups.Count == 0) 
                return;
            var popup = _popups.Peek();
            popup.PopupCentered();
        }

        private void Spawn(WindowDialog popup)
        {
            AddChild(popup);

            if (_popups.Count == 0)
                popup.PopupCentered();

            _popups.Enqueue(popup);
        }
    }
}

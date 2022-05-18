using Godot;

namespace GodotModules
{
    public class PopupManager : Node
    {
        private Queue<WindowDialog> _popups = new();

        public void SpawnPopupMessage(string message, string title = "")
        {
            var popupMessage = Prefabs.UIPopupMessage.Instance<global::PopupMessage>();

            popupMessage.PreInit(this, message, title);
            AddChild(popupMessage);

            if (_popups.Count == 0)
                popupMessage.PopupCentered();

            _popups.Enqueue(popupMessage);
        }

        public void SpawnPopupError(Exception exception, string title = "")
        {
            var popupError = Prefabs.UIPopupError.Instance<global::PopupError>();

            popupError.PreInit(this, exception, title);
            AddChild(popupError);

            if (_popups.Count == 0)
                popupError.PopupCentered();
                
            _popups.Enqueue(popupError);
        }

        public void SpawnNextPopup() 
        {
            _popups.Dequeue();
            if (_popups.Count == 0) 
                return;
            var popup = _popups.Peek();
            popup.PopupCentered();
        }
    }
}

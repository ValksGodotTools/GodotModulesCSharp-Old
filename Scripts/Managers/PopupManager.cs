using Godot;

namespace GodotModules
{
    public class PopupManager
    {
        private Queue<WindowDialog> _queue = new();
        private Node _popups;

        public PopupManager(Node popups)
        {
            _popups = popups;
        }

        public void SpawnMessage(string message, string title = "")
        {
            var popupMessage = Prefabs.UIPopupMessage.Instance<PopupMessage>();
            popupMessage.PreInit(this, message, title);
            
            Spawn(popupMessage);
        }

        public void SpawnError(Exception exception, string title = "")
        {
            var popupError = Prefabs.UIPopupError.Instance<PopupError>();
            popupError.PreInit(this, exception, title);
            
            Spawn(popupError);
        }

        public void Next() 
        {
            _queue.Dequeue();
            if (_queue.Count == 0) 
                return;
            var popup = _queue.Peek();
            popup.PopupCentered();
        }

        private void Spawn(WindowDialog popup)
        {
            _popups.AddChild(popup);

            if (_queue.Count == 0)
                popup.PopupCentered();

            _queue.Enqueue(popup);
        }
    }
}

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
            var popup = Prefabs.PopupMessage.Instance<PopupMessage>();
            popup.PreInit(this, message, title);
            
            Spawn(popup);
        }

        public void SpawnError(Exception exception, string title = "")
        {
            var popup = Prefabs.PopupError.Instance<PopupError>();
            popup.PreInit(this, exception, title);
            
            Spawn(popup);
        }

        public void SpawnLineEdit(Action code, string title = "") 
        {
            
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

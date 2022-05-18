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

        public void SpawnLineEdit(Action<LineEdit> onTextChanged, Action<string> onHide, int maxLength = 50, string title = "") 
        {
            var popup = Prefabs.PopupLineEdit.Instance<PopupLineEdit>();
            popup.PreInit(this, onTextChanged, onHide, maxLength, title);

            Spawn(popup);
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
                popup.PopupCentered(popup.RectMinSize);

            _queue.Enqueue(popup);
        }
    }
}

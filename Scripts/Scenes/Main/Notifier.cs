using Godot;
using System;

namespace GodotModules
{
    public class Notifier : Node
    {
        private int _errorCount;

        public override void _Ready()
        {
            var timer = new GTimer(1500);
            timer.Connect(this, nameof(SpawnErrorNotification));
        }

        public void IncrementErrorCount() => _errorCount++;

        public void SpawnErrorNotification()
        {
            if (_errorCount == 0)
                return;

            var notifyError = Prefabs.NotifyError.Instance<UINotifyError>();
            notifyError.Count = _errorCount;
            GM.PersistentTree.CurrentScene.AddChild(notifyError);

            _errorCount = 0;
        }

        public void SpawnPopupMessage(string message)
        {
            var popupMessage = Prefabs.PopupMessage.Instance<UIPopupMessage>();
            GM.PersistentTree.CurrentScene.AddChild(popupMessage);
            popupMessage.Init(message);
            popupMessage.PopupCentered();
        }

        public void SpawnPopupError(Exception e)
        {
            IncrementErrorCount();
            var popupError = Prefabs.PopupError.Instance<UIPopupError>();
            GM.PersistentTree.CurrentScene.AddChild(popupError);
            popupError.Init(e.Message, e.StackTrace);
            popupError.PopupCentered();
        }
    }
}
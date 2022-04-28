using Godot;

namespace GodotModules
{
    public class ErrorNotifier : Node
    {
        private static int ErrorCount { get; set; }
        public static void IncrementErrorCount() => ErrorCount++;

        public override void _Ready()
        {
            var timer = new Timer();
            timer.Connect("timeout", this, nameof(SpawnErrorNotification));
            timer.WaitTime = 1.5f;
            timer.OneShot = false;
            timer.Autostart = true;
            AddChild(timer);
        }

        private static void SpawnErrorNotification()
        {
            if (ErrorCount == 0)
                return;

            var notifyError = Prefabs.NotifyError.Instance<UINotifyError>();
            notifyError.Count = ErrorCount;
            GameManager.Instance.AddChild(notifyError);

            ErrorCount = 0;
        }
    }
}
using Godot;

namespace GodotModules
{
    public class ErrorNotifier : Node
    {
        private static int ErrorCount { get; set; }
        public static void IncrementErrorCount() => ErrorCount++;

        private static GTimer Timer { get; set; }

        public override void _Ready()
        {
            Timer = new GTimer(1500);
            Timer.Connect(this, nameof(SpawnErrorNotification));
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
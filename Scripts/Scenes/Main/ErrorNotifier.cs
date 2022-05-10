using Godot;

namespace GodotModules
{
    public class ErrorNotifier : Node
    {
        private static int ErrorCount { get; set; }

        public static void IncrementErrorCount() => ErrorCount++;

        private static GTimer TimerNotifyErrors { get; set; }

        public override void _Ready()
        {
            TimerNotifyErrors = new GTimer(1500);
            TimerNotifyErrors.Connect(this, nameof(SpawnErrorNotification));
        }

        private static void SpawnErrorNotification()
        {
            if (ErrorCount == 0)
                return;

            var notifyError = Prefabs.NotifyError.Instance<UINotifyError>();
            notifyError.Count = ErrorCount;
            GM.PersistentTree.CurrentScene.AddChild(notifyError);

            ErrorCount = 0;
        }
    }
}
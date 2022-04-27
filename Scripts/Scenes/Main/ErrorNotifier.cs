using System.Timers;

namespace GodotModules
{
    public static class ErrorNotifier
    {
        private static Timer TimerErrorNotifier { get; set; }
        private static int ErrorCount { get; set; }

        public static void IncrementErrorCount() => ErrorCount++;

        public static void StartInterval(int seconds)
        {
            TimerErrorNotifier = new(seconds * 1000);
            TimerErrorNotifier.Elapsed += SpawnErrorNotification;
            TimerErrorNotifier.AutoReset = true;
            TimerErrorNotifier.Enabled = true;
        }

        public static void Dispose() => TimerErrorNotifier.Dispose();

        private static void SpawnErrorNotification(System.Object source, ElapsedEventArgs args)
        {
            if (ErrorCount == 0)
                return;

            var notifyError = Prefabs.NotifyError.Instance<UINotifyError>();
            notifyError.Init(ErrorCount);
            GameManager.Instance.CallDeferred("add_child", notifyError);

            ErrorCount = 0;
        }
    }
}
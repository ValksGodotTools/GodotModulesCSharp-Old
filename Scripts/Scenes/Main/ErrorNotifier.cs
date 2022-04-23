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
            TimerErrorNotifier = new Timer(seconds * 1000);
            TimerErrorNotifier.Elapsed += SpawnErrorNotification;
            TimerErrorNotifier.AutoReset = true;
            TimerErrorNotifier.Enabled = true;
        }

        private static void SpawnErrorNotification(System.Object source, ElapsedEventArgs args)
        {
            if (ErrorCount == 0)
                return;

            GameManager.NotifyError(ErrorCount);

            ErrorCount = 0;
        }
    }
}
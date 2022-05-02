using System;
using System.Timers;

namespace GodotModules
{
    public class STimer : IDisposable
    {
        private Timer Timer { get; set; }

        public STimer(double delayMs, Action action, bool enabled = true, bool autoreset = true)
        {
            void Callback(Object source, ElapsedEventArgs e) => action();
            Timer = new Timer(delayMs);
            Timer.Enabled = enabled;
            Timer.AutoReset = autoreset;
            Timer.Elapsed += Callback;
        }

        public void Start() => Timer.Enabled = true;
        public void Stop() => Timer.Enabled = false;

        public void Dispose() => Timer.Dispose();
    }
}
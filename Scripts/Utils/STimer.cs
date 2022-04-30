using System;
using System.Threading;

namespace GodotModules
{
    public class STimer : IDisposable
    {
        private Timer Timer { get; set; }

        public STimer(int delayMs, Action action)
        {
            void Callback(object state) => action();
            Timer = new Timer(new TimerCallback(Callback), "state", delayMs, delayMs);
        }

        public void Dispose() => Timer.Dispose();
    }
}
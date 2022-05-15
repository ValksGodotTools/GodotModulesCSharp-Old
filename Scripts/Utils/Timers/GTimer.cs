using Godot;

namespace GodotModules
{
    public class GTimer
    {
        private readonly Timer _timer = new Timer();

        public GTimer(Node target, string methodName, int delayMs = 1000, bool loop = true, bool autoStart = true)
        {
            _timer.WaitTime = delayMs / 1000f;
            _timer.OneShot = !loop;
            _timer.Autostart = autoStart;
            _timer.Connect("timeout", target, methodName);
            target.AddChild(_timer);
        }

        public void Start(float delayMs)
        {
            _timer.WaitTime = delayMs / 1000;
            Start();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void QueueFree() => _timer.QueueFree();
    }
}
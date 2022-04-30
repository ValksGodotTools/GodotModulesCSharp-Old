using Godot;
using System;

namespace GodotModules
{
    public class GTimer
    {
        private Timer Timer = new Timer();

        public GTimer(int delayMs, bool loop = true, bool autoStart = true)
        {
            Timer.WaitTime = delayMs / 1000f;
            Timer.OneShot = !loop;
            Timer.Autostart = autoStart;
        }

        public void Connect(Node target, string methodName)
        {
            Timer.Connect("timeout", target, methodName);
            target.AddChild(Timer);
        }

        public void Start(float delayMs) 
        {
            Timer.WaitTime = delayMs / 1000;
            Start();
        }
        
        public void Start() => Timer.Start();
        public void Stop() => Timer.Stop();
        public void QueueFree() => Timer.QueueFree();
    }
}
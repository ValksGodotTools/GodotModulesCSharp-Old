using Godot;

namespace GodotModules
{
    public class PrevCurQueue<T>
    {
        private List<T> Data = new();
        public float Progress { get; private set; }

        public T Previous => Data[0];
        public T Current => Data[1];

        public bool NotReady => Data.Count <= 1;

        private int Interval { get; set; }

        public PrevCurQueue(int interval) 
        {
            Interval = interval;
        }

        public void Add(T data)
        {
            Progress = 0; // reset progress as this is new incoming data
            Data.Add(data);

            if (Data.Count > 2) // only keep track of previous and current
                Data.RemoveAt(0);
        }

        public void UpdateProgress(float delta) => Progress += delta * (1000f / Interval); // reach value of 1.0 every 150ms
    }
}
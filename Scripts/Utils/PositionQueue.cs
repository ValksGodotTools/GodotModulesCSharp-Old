using Godot;

namespace GodotModules
{
    public class PositionQueue
    {
        private List<Dictionary<uint, Vector2>> Data = new();
        public float Progress { get; private set; }

        public Dictionary<uint, Vector2> Previous => Data[0];
        public Dictionary<uint, Vector2> Current => Data[1];

        public bool NotReady => Data.Count <= 1;

        public void Add(Dictionary<uint, Vector2> data)
        {
            Progress = 0; // reset progress as this is new incoming data
            Data.Add(data);

            if (Data.Count > 2) // only keep track of previous and current
                Data.RemoveAt(0);
        }

        public void UpdateProgress(float delta) => Progress += delta * (1000f / CommandDebug.SendReceiveDataInterval); // reach value of 1.0 every 200ms
    }
}
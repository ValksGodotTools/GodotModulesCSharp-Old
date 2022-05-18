using Godot;

namespace GodotModules 
{
    public class ErrorNotifierManager : Node
    {
        private int _errorCount;

        public override void _Ready() =>
            new GTimer(this, nameof(SpawnErrorNotification), 1500);

        public void IncrementErrorCount() => 
            _errorCount++;

        public void SpawnErrorNotification()
        {
            if (_errorCount == 0)
                return;

            var notifyError = Prefabs.UIErrorNotifier.Instance<UIErrorNotifier>();
            notifyError.Count = _errorCount;
            AddChild(notifyError);

            _errorCount = 0;
        }
    }
}

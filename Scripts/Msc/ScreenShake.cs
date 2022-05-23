using Godot;

namespace GodotModules
{
    public class ScreenShake : Node
    {
        [Export] protected readonly NodePath NodePathShakeTween;
        private Tween _shakeTween;
        private Camera2D _camera2D;
        private GTimer _frequency;
        private GTimer _duration;
        private float _amplitude;

        public override void _Ready()
        {
            _shakeTween = GetNode<Tween>(NodePathShakeTween);
            _camera2D = (Camera2D)GetParent();
            _frequency = new(this, nameof(FrequencyTimeout), 1000, true, false);
            _duration = new(this, nameof(DurationTimeout), 200, false, false);
        }

        public void Start(int duration = 200, int frequency = 25, float amplitude = 1000)
        {
            _amplitude = amplitude;
            _duration.StartMs(duration);
            _frequency.Start(1 / (float)frequency);

            NewShake();
        }

        public void NewShake()
        {
            var rand = new Vector2();
            rand.x = (float)GD.RandRange(-1, 1) * _amplitude;
            rand.y = (float)GD.RandRange(-1, 1) * _amplitude;

            _shakeTween.InterpolateProperty(_camera2D, "offset", _camera2D.Offset, rand, _frequency.Delay, Tween.TransitionType.Sine);
            _shakeTween.Start();
        }

        public void Reset()
        {
            _shakeTween.InterpolateProperty(_camera2D, "offset", _camera2D.Offset, Vector2.Zero, _frequency.Delay, Tween.TransitionType.Sine);
            _shakeTween.Start();
        }

        private void FrequencyTimeout()
        {
            NewShake();
        }

        private void DurationTimeout()
        {
            Reset();
            _frequency.Stop();
        }
    }
}

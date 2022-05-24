using Godot;

namespace GodotModules
{
    public class ADS : Spatial
    {
        [Export] protected Vector3 RestPosition;
        [Export] protected Vector3 ADSPosition;
        [Export] protected readonly NodePath NodePathCamera;

        private Camera _camera;
        private Dictionary<string, float> _fov;

        public override void _Ready()
        {
            _camera = GetNode<Camera>(NodePathCamera);
            _fov = new(){
                { "Rest", _camera.Fov },
                { "ADS", _camera.Fov - 20 }
            };
        }

        public override void _Process(float delta)
        {
            var transform = Transform;

            if (Input.IsActionPressed("player_aim")) 
            {
                transform.origin = transform.origin.LinearInterpolate(ADSPosition, 20 * delta);
                _camera.Fov = _camera.Fov.Lerp(_fov["ADS"], 20 * delta);
            }
            else
            {
                transform.origin = transform.origin.LinearInterpolate(RestPosition, 20 * delta);
                _camera.Fov = _camera.Fov.Lerp(_fov["Rest"], 20 * delta);
            }

            Transform = transform;
        }
    }
}

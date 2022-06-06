using MouseMode = Godot.Input.MouseMode;

namespace GodotModules
{
    public class FPSController : KinematicBody
    {
        [Export] protected readonly NodePath NodePathHead;
        [Export] protected readonly NodePath NodePathHand;
        [Export] protected readonly NodePath NodePathSkeleton;

        private Spatial _head;
        private Spatial _hand;

        private float _mouseSensitivity = 0.10f;

        public override void _Ready()
        {
            _head = GetNode<Spatial>(NodePathHead);
            _hand = GetNode<Spatial>(NodePathHand);

            Input.SetMouseMode(MouseMode.Captured);

            Notifications.AddListener(this, Event.OnMouseMotionInput, OnMouseMotionInput);
            Notifications.AddListener(this, Event.OnMouseButtonInput, OnMouseButtonInput);
            Notifications.AddListener(this, Event.OnKeyboardInput, OnKeyboardInput);
        }

        private void OnMouseMotionInput(Node sender, object[] args)
        {
            var motion = (InputEventMouseMotion)args[0];

            if (Input.GetMouseMode() == MouseMode.Captured)
            {
                RotateY((-motion.Relative.x * _mouseSensitivity).ToRadians());
                _head.RotateX((motion.Relative.y * _mouseSensitivity).ToRadians());
            }
        }

        private void OnMouseButtonInput(Node sender, object[] args)
        {
            var button = (InputEventMouseButton)args[0];

            if (button.ButtonIndex == (int)ButtonList.Left && Input.GetMouseMode() == MouseMode.Visible)
                Input.SetMouseMode(MouseMode.Captured);
        }

        private void OnKeyboardInput(Node sender, object[] args) 
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                Input.SetMouseMode(MouseMode.Visible);
        }
    }
}

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
        private Skeleton _skeleton;

        private float _mouseSensitivity = 0.10f;

        public override void _Ready()
        {
            _head = GetNode<Spatial>(NodePathHead);
            _hand = GetNode<Spatial>(NodePathHand);
            _skeleton = GetNode<Skeleton>(NodePathSkeleton);

            Input.SetMouseMode(MouseMode.Captured);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion motion)
                if (Input.GetMouseMode() == MouseMode.Captured)
                {
                    RotateY((-motion.Relative.x * _mouseSensitivity).ToRadians());
                    _head.RotateX((motion.Relative.y * _mouseSensitivity).ToRadians());
                }

            if (@event is InputEventMouseButton button)
                if (button.ButtonIndex == (int)ButtonList.Left && Input.GetMouseMode() == MouseMode.Visible)
                    Input.SetMouseMode(MouseMode.Captured);

            if (Input.IsActionJustPressed("ui_cancel"))
                Input.SetMouseMode(MouseMode.Visible);
        }
    }
}

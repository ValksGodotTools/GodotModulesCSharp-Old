namespace GodotModules;

public class ControlInventoryItemCursor : Node2D
{
    public override void _PhysicsProcess(float delta)
    {
        var parent = (Node2D)GetParent();
        parent.GlobalPosition = GetGlobalMousePosition();
    }
}

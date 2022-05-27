namespace GodotModules
{
    public class InventoryItemCursor : TextureRect
    {
        public override void _PhysicsProcess(float delta)
        {
            RectGlobalPosition = GetGlobalMousePosition() - RectSize / 2;
        }
    }
}

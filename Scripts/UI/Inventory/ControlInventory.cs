namespace GodotModules
{
    public class ControlInventory : Control
    {
        [Export] protected readonly NodePath NodePathGridContainer;
        [Export] protected readonly NodePath NodePathGroupCursor;

        public GridContainer GridContainer;
        public Dictionary<int, Dictionary<int, InventoryItem>> Items = new Dictionary<int, Dictionary<int, InventoryItem>>();
        public bool HoldingItem;

        private Node _groupCursor;

        public override void _Ready()
        {
            _groupCursor = GetNode<Node>(NodePathGroupCursor);

            // Inventory size
            var invItemSize = 52;
            var columns = 10;
            var rows = 6;
            RectSize = new Vector2(invItemSize * columns, invItemSize * rows);

            GridContainer = GetNode<GridContainer>(NodePathGridContainer);
            GridContainer.Columns = columns;

            for (int c = 0; c < columns; c++)
            {
                Items[c] = new Dictionary<int, InventoryItem>();

                for (int r = 0; r < rows; r++)
                {
                    var item = new InventoryItem(this);
                    Items[c][r] = item;
                }
            }

            Items[0][0].SetItem("MiniGodotChan", 10);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)ButtonList.Left&& mouseButton.Pressed) 
            {
                if (HoldingItem) 
                {
                    HoldingItem = false;
                    _groupCursor.GetChild(0).QueueFree();
                }
            }
        }

        public void HoldItem(Node2D item)
        {
            HoldingItem = true;
            _groupCursor.AddChild(item);
        }
    }
}

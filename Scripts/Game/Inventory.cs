namespace GodotModules
{
    public class Inventory : Control
    {
        [Export] protected readonly NodePath NodePathGridContainer;
        [Export] protected readonly NodePath NodePathGroupCursor;

        private GridContainer _gridContainer;

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

            _gridContainer = GetNode<GridContainer>(NodePathGridContainer);
            _gridContainer.Columns = columns;

            for (int c = 0; c < columns; c++)
            {
                Items[c] = new Dictionary<int, InventoryItem>();

                for (int r = 0; r < rows; r++)
                {
                    var item = Prefabs.InventoryItem.Instance<InventoryItem>();
                    item.Init(this);
                    if (GD.Randf() < 0.5f)
                        item.ItemType = Item.MiniGodotChan;
                    else
                        item.ItemType = Item.Coin;
                    _gridContainer.AddChild(item);
                    Items[c][r] = item;
                }
            }
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

        public void HoldItem(InventoryItemCursor item)
        {
            HoldingItem = true;
            _groupCursor.AddChild(item);
        }
    }

    public enum Item 
    {
        MiniGodotChan,
        Coin
    }
}

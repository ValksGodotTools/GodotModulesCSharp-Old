namespace GodotModules
{
    public class ControlInventoryItem : Control
    {
        [Export] protected readonly NodePath NodePathItem;
        [Export] protected readonly NodePath NodePathStackSize;

        private Node _itemParent;
        private Label _stackSize;

        private ControlInventory _inventory;

        private Vector2 _invItemSize = new Vector2(50, 50);
        private Vector2 _itemSize = new Vector2(20, 20);

        public override void _Ready()
        {
            _itemParent = GetNode<Node>(NodePathItem);
            _stackSize = GetNode<Label>(NodePathStackSize);
        }

        public void Init(ControlInventory inventory)
        {
            _inventory = inventory;
        }

        public void SetItem(string name, int stackSize)
        {
            if (Items.Sprites.ContainsKey(name))
                SetSprite(name);
            else if (Items.AnimatedSprites.ContainsKey(name))
                SetAnimatedSprite(name);
            else
                throw new InvalidOperationException($"The item '{name}' does not exist as a sprite or animated sprite");

            _stackSize.Text = $"{stackSize}";
        }

        private void _on_Item_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
                {
                    if (_inventory.HoldingItem)
                        return;

                    //_textureRect.Texture = null;
                    //var item = Prefabs.InventoryItemCursor.Instance<InventoryItemCursor>();
                    //_inventory.HoldItem(item);
                }
            }
        }

        private void SetSprite(string name)
        {
            ClearItem();

            var sprite = new Sprite();
            sprite.Texture = Items.Sprites[name];
            sprite.Position += _invItemSize / 2;
            sprite.Scale = _itemSize / sprite.Texture.GetSize();
            _itemParent.AddChild(sprite);
        }

        private void SetAnimatedSprite(string name)
        {
            ClearItem();

            var sprite = new AnimatedSprite();
            sprite.Frames = Items.AnimatedSprites[name];
            sprite.Playing = true;
            sprite.Position += _invItemSize / 2;
            sprite.Scale = _itemSize / sprite.Frames.GetFrame("default", 0).GetSize();
            _itemParent.AddChild(sprite);
        }

        private void ClearItem()
        {
            foreach (Node child in _itemParent.GetChildren())
                child.QueueFree();
        }
    }
}

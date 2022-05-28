namespace GodotModules
{
    public class ControlInventoryItem : Control
    {
        [Export] protected readonly NodePath NodePathItem;

        private Node _item;

        public Item ItemType { get; set; }

        private ControlInventory _inventory;

        private Vector2 _invItemSize = new Vector2(50, 50);
        private Vector2 _itemSize = new Vector2(20, 20);

        public override void _Ready()
        {
            _item = GetNode<Node>(NodePathItem);
        }

        public void SetSprite()
        {
            var sprite = new Sprite();
            sprite.Texture = Textures.MiniGodotChan;
            sprite.Position += _invItemSize / 2;
            sprite.Scale = _itemSize / sprite.Texture.GetSize();
            _item.AddChild(sprite);
        }

        public void SetAnimatedSprite()
        {
            var sprite = new AnimatedSprite();
            sprite.Frames = Textures.Coin;
            sprite.Playing = true;
            sprite.Position += _invItemSize / 2;
            sprite.Scale = _itemSize / sprite.Frames.GetFrame("default", 0).GetSize();
            _item.AddChild(sprite);
        }

        public void Init(ControlInventory inventory)
        {
            _inventory = inventory;
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
    }
}

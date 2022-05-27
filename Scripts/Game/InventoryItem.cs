namespace GodotModules
{
    public class InventoryItem : Control
    {
        public Item Item { get; set; }

        private Inventory _inventory;

        public override void _Ready()
        {
            var invItemSize = new Vector2(50, 50);
            var itemSize = new Vector2(25, 25);

            if (Item == Item.MiniGodotChan)
            {
                var sprite = new Sprite();
                sprite.Texture = Textures.MiniGodotChan;
                sprite.Position += invItemSize / 2;
                sprite.Scale = itemSize / sprite.Texture.GetSize();
                AddChild(sprite);
            }

            if (Item == Item.Coin)
            {
                var sprite = new AnimatedSprite();
                sprite.Frames = Textures.Coin;
                sprite.Playing = true;
                sprite.Position += invItemSize / 2;
                sprite.Scale = itemSize / sprite.Frames.GetFrame("default", 0).GetSize();
                AddChild(sprite);
            }
        }

        public void Init(Inventory inventory)
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

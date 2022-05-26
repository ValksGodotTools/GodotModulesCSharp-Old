namespace GodotModules
{
    public class InventoryItem : Control
    {
        [Export] protected readonly NodePath NodePathTextureRect;

        public Item Item { get; set; }

        private TextureRect _textureRect;
        private Inventory _inventory;

        public override void _Ready()
        {
            _textureRect = GetNode<TextureRect>(NodePathTextureRect);

            if (Item == Item.MiniGodotChan)
            {
                _textureRect.Texture = Textures.MiniGodotChan;
            }

            if (Item == Item.Coin)
            {
                //_textureRect.Texture = Textures.
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

                    _textureRect.Texture = null;
                    var item = Prefabs.InventoryItemCursor.Instance<InventoryItemCursor>();
                    _inventory.HoldItem(item);
                }
            }
        }
    }
}

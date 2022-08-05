namespace GodotModules;

public class ControlInventoryItem : Control
{
    [Export] protected readonly NodePath NodePathContent;
    [Export] protected readonly NodePath NodePathItem;
    [Export] protected readonly NodePath NodePathStackSize;

    private Control _content;
    private Node _itemParent;
    private Label _stackSize;

    private ControlInventory _inventory;
    private InventoryItem _item;

    private Vector2 _invItemSize = new Vector2(50, 50);
    private Vector2 _itemSize = new Vector2(20, 20);

    public override void _Ready()
    {
        _content = GetNode<Control>(NodePathContent);
        _itemParent = GetNode<Node>(NodePathItem);
        _stackSize = GetNode<Label>(NodePathStackSize);
    }

    public void Init(ControlInventory inventory)
    {
        _inventory = inventory;
    }

    public void InitItem(InventoryItem item)
    {
        _item = item;
        SetStackSize(item);
        SetImage();
    }

    private void SetStackSize(InventoryItem item)
    {
        _item.StackSize = item.StackSize;
        _stackSize.Text = $"{_item.StackSize}";
    }

    private void SetImage()
    {
        if (_item.Type == InventoryItemType.Static)
            SetSprite(_item.Name);
        else if (_item.Type == InventoryItemType.Animated)
            SetAnimatedSprite(_item.Name);
    }

    public void SetItem(InventoryItem otherItem)
    {
        _item.Name = otherItem.Name;
        _item.Type = otherItem.Type;
        SetStackSize(otherItem);
        SetImage();
    }

    public void RemoveItem()
    {
        _content.Visible = true;
        ClearItem();
    }

    private void _on_Item_gui_input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton || mouseButton.ButtonIndex != (int)ButtonList.Left || !mouseButton.Pressed)
            return;

        Logger.LogDebug($"{_item?.Name} {_item?.X} {_item?.Y}");

        if (_inventory.IsHoldingItem)
        {
            if (!_inventory.HoldingItem.Equals(_item)) // position is not the same, lets move the currently held item to this slot
            {
                _inventory.HoldingItem.SetItem(_item);

                SetItem(_inventory.HoldingItem);

                ClearCursorItem();

                _inventory.IsHoldingItem = false;
            }
        }

        // Pick up item (put item on mouse cursor position)
        if (_inventory.HoldingItem == null)
            Pickup();
    }

    private void Pickup()
    {
        _content.Visible = false;
        _inventory.IsHoldingItem = true;
        _inventory.HoldingItem = _item;

        if (_item.Type == InventoryItemType.Static)
        {
            var sprite = InitSprite(_item.Name);
            var controlInventoryItemCursor = Prefabs.InventoryItemCursor.Instance<ControlInventoryItemCursor>();
            sprite.AddChild(controlInventoryItemCursor);
            _inventory.CursorParent.AddChild(sprite);
        }
        else if (_item.Type == InventoryItemType.Animated)
        {
            var animatedSprite = InitAnimatedSprite(_item.Name);
            var controlInventoryItemCursor = Prefabs.InventoryItemCursor.Instance<ControlInventoryItemCursor>();
            animatedSprite.AddChild(controlInventoryItemCursor);
            _inventory.CursorParent.AddChild(animatedSprite);
        }
    }

    private void ClearCursorItem()
    {
        foreach (Node child in _inventory.CursorParent.GetChildren())
            child.QueueFree();
    }

    private void SetSprite(string name)
    {
        RemoveItem();

        var sprite = InitSprite(name);
        _itemParent.AddChild(sprite);
    }

    private Sprite InitSprite(string name)
    {
        var sprite = new Sprite();
        sprite.Texture = Items.Sprites[name];
        sprite.Position += _invItemSize / 2;
        sprite.Scale = _itemSize / sprite.Texture.GetSize();
        return sprite;
    }

    private void SetAnimatedSprite(string name)
    {
        RemoveItem();

        var animatedSprite = InitAnimatedSprite(name);
        _itemParent.AddChild(animatedSprite);
    }

    private AnimatedSprite InitAnimatedSprite(string name)
    {
        var animatedSprite = new AnimatedSprite();
        animatedSprite.Frames = Items.AnimatedSprites[name];
        animatedSprite.Playing = true;
        animatedSprite.Position += _invItemSize / 2;
        animatedSprite.Scale = _itemSize / animatedSprite.Frames.GetFrame("default", 0).GetSize();
        return animatedSprite;
    }

    private void ClearItem()
    {
        foreach (Node child in _itemParent.GetChildren())
            child.QueueFree();

        _stackSize.Text = "";
    }
}

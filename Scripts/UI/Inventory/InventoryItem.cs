namespace GodotModules 
{
    public class InventoryItem 
    {
        public string Name { get; set; }
        public int StackSize { get; set; }
        public InventoryItemType Type { get; set; }

        private ControlInventoryItem _controlInventoryItem;

        public InventoryItem(ControlInventory controlInventory)
        {
            _controlInventoryItem = Prefabs.InventoryItem.Instance<ControlInventoryItem>();
            _controlInventoryItem.Init(controlInventory);
            controlInventory.GridContainer.AddChild(_controlInventoryItem);
        }

        public void SetItem(string name, int stackSize = 1) 
        {
            Name = name;
            StackSize = stackSize;

            if (Items.Sprites.ContainsKey(name))
                Type = InventoryItemType.Static;
            else if (Items.AnimatedSprites.ContainsKey(name))
                Type = InventoryItemType.Animated;
            else
                throw new InvalidOperationException($"The item '{name}' does not exist as a sprite or animated sprite");
            
            _controlInventoryItem.SetItem(this);
        }
    }

    public enum InventoryItemType 
    {
        Static,
        Animated
    }
}
namespace GodotModules 
{
    public class InventoryItem 
    {
        private ControlInventoryItem _controlInventoryItem;

        public InventoryItem(ControlInventory controlInventory)
        {
            _controlInventoryItem = Prefabs.InventoryItem.Instance<ControlInventoryItem>();
            _controlInventoryItem.Init(controlInventory);
            controlInventory.GridContainer.AddChild(_controlInventoryItem);
        }

        public void SetItem(string name) => _controlInventoryItem.SetItem(name);
    }
}
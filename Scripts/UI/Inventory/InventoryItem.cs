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
            _controlInventoryItem.SetSprite();
        }

        public void SetItem()
        {

        }
    }
}
public class ChestUI : InventoryUI
{
    Chest chestInventory;
    public Chest ChestInventory => chestInventory;

    public void SetChest(Chest chest)
    {
        chestInventory = chest;

        SlotPanel.SetInventory(chest);
        foreach (var forwarder in forwarders)
        {
            SlotEventBridge.Subscribe(forwarder);
        }
    }
    public void ClearChest()
    {
        chestInventory = null;
    }
}
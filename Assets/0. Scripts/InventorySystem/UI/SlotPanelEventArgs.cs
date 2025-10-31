using UnityEngine.EventSystems;

public readonly struct SlotPanelEventArgs
{
    public SlotPanelEventArgs(InventorySlotUI slot, StorageTarget source, PointerEventData pointer, StoredItem item)
    {
        Slot = slot;
        Source = source;
        Pointer = pointer;
        Item = item;
    }

    public InventorySlotUI Slot { get; }
    public StorageTarget Source { get; }
    public PointerEventData Pointer { get; }
    public StoredItem Item { get; }
}

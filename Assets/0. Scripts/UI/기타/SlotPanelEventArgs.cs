using UnityEngine;
using UnityEngine.EventSystems;

public readonly struct SlotPanelEventArgs
{
    public SlotPanelEventArgs(StoredItem item, StorageTarget source, RectTransform rect, PointerEventData pointer)
    {
        Item = item;
        Source = source;
        Rect = rect;
        Pointer = pointer;
    }

    public StoredItem Item { get; }
    public StorageTarget Source { get; }
    public RectTransform Rect { get; }
    public PointerEventData Pointer { get; }
    
}

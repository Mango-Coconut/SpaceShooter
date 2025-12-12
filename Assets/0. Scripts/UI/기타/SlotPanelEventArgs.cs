using UnityEngine;
using UnityEngine.EventSystems;

public readonly struct SlotPanelEventArgs
{
    public SlotPanelEventArgs(SlotEventArgs<StoredItem> baseArgs, StorageTarget source)
    {
        Item = baseArgs.Data;
        Source = source;
        Rect = baseArgs.Rect;
        Pointer = baseArgs.Pointer;
    }

    public StoredItem Item { get; }
    public StorageTarget Source { get; }
    public RectTransform Rect { get; }
    public PointerEventData Pointer { get; }

}

using System;
using UnityEngine.EventSystems;

public interface ISlotPanel
{
    void Refresh();
    
    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI> RightClick;
    public event Action<InventorySlotUI, IItemSource, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

}

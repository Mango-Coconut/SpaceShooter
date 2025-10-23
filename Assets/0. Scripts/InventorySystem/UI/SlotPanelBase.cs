using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SlotPanelBase : MonoBehaviour
{
    protected List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI, IItemSource, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

    protected abstract IItemSource GetSource(); // Inventory or EquipInventory

    protected virtual void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;
            slot.handler.PointerEnter += HandlePointerEnter;
            slot.handler.PointerExit += HandlePointerExit;
            slot.handler.BeginDragSlot += HandleBeginDrag;
            slot.handler.DragSlot += HandleDrag;
            slot.handler.EndDragSlot += HandleEndDrag;
        }
    }

    protected virtual void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;
            slot.handler.PointerEnter -= HandlePointerEnter;
            slot.handler.PointerExit -= HandlePointerExit;
            slot.handler.BeginDragSlot -= HandleBeginDrag;
            slot.handler.DragSlot -= HandleDrag;
            slot.handler.EndDragSlot -= HandleEndDrag;
        }
    }

    void HandlePointerEnter(InventorySlotUI slotUI) => TooltipShown?.Invoke(slotUI);
    void HandlePointerExit(InventorySlotUI slotUI) => TooltipHidden?.Invoke(slotUI);

    void HandleBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        TooltipHidden?.Invoke(slotUI);
        BeginDrag?.Invoke(slotUI, GetSource(), e);
    }

    void HandleDrag(InventorySlotUI slotUI, PointerEventData e) => Dragging?.Invoke(slotUI, e);

    void HandleEndDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Dropped?.Invoke(slotUI, e);
    }
}
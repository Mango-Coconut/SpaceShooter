using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SlotPanelBase : MonoBehaviour
{
    protected List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    
    public virtual bool IsInteractable => isActiveAndEnabled;
    public IEnumerable<InventorySlotUI> Slots => uiSlots;


    public event Action<SlotPanelEventArgs> OnMouseEnter;
    public event Action<SlotPanelEventArgs> OnMouseExit;
    public event Action<SlotPanelEventArgs> OnRightClickArgs;
    public event Action<SlotPanelEventArgs> OnBeginDragArgs;
    public event Action<SlotPanelEventArgs> OnDraggingArgs;
    public event Action<SlotPanelEventArgs> OnDroppedArgs;

    protected abstract StorageTarget GetSource(); // Inventory or EquipInventory


    protected virtual void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;
            slot.handler.PointerEnter += ForwardMouseEnter;
            slot.handler.PointerExit += ForwardMouseExit;
            slot.handler.RightClick += ForwardRightClick;
            slot.handler.BeginDrag += ForwardBeginDrag;
            slot.handler.Dragging += ForwardDragging;
            slot.handler.EndDrag += ForwardDropped;
        }
    }

    protected virtual void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;
            slot.handler.PointerEnter -= ForwardMouseEnter;
            slot.handler.PointerExit -= ForwardMouseExit;
            slot.handler.RightClick -= ForwardRightClick;
            slot.handler.BeginDrag -= ForwardBeginDrag;
            slot.handler.Dragging -= ForwardDragging;
            slot.handler.EndDrag -= ForwardDropped;
        }
    }

    void ForwardMouseEnter(InventorySlotUI slotUI)
    {
        OnMouseEnter?.Invoke(new SlotPanelEventArgs(slotUI, GetSource(), null, slotUI != null ? slotUI.EnterItem : null));
    }
    void ForwardMouseExit()
    {
        OnMouseExit?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
    }
    public void ForwardRightClick(StoredItem item)
    {
        OnRightClickArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, item));
    }
    
    void ForwardBeginDrag(StoredItem item, PointerEventData e)
    {
        OnMouseExit?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
        OnBeginDragArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, item));
    }

    void ForwardDragging(PointerEventData e)
    {
        OnDraggingArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, null));
    }

    void ForwardDropped(StoredItem item, PointerEventData e)
    {
        OnDroppedArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, item));
    }
}

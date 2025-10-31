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
            slot.handler.PointerEnter += OnPointerEnter;
            slot.handler.PointerExit += OnPointerExit;
            slot.handler.RightClick += OnPointerRightClick;
            slot.handler.BeginDrag += OnBeginDrag;
            slot.handler.Dragging += OnDragging;
            slot.handler.EndDrag += OnEndDrag;
        }
    }

    protected virtual void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;
            slot.handler.PointerEnter -= OnPointerEnter;
            slot.handler.PointerExit -= OnPointerExit;
            slot.handler.RightClick -= OnPointerRightClick;
            slot.handler.BeginDrag -= OnBeginDrag;
            slot.handler.Dragging -= OnDragging;
            slot.handler.EndDrag -= OnEndDrag;
        }
    }

    void OnPointerEnter(InventorySlotUI slotUI)
    {
        OnMouseEnter?.Invoke(new SlotPanelEventArgs(slotUI, GetSource(), null, slotUI != null ? slotUI.EnterItem : null));
    }
    void OnPointerExit()
    {
        OnMouseExit?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
    }
    public void OnPointerRightClick(StoredItem item)
    {
        OnRightClickArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, item));
    }
    
    void OnBeginDrag(StoredItem item, PointerEventData e)
    {
        OnMouseExit?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
        OnBeginDragArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, item));
    }

    void OnDragging(PointerEventData e)
    {
        OnDraggingArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, null));
    }

    void OnEndDrag(StoredItem item, PointerEventData e)
    {
        OnDroppedArgs?.Invoke(new SlotPanelEventArgs(null, GetSource(), e, item));
    }
}

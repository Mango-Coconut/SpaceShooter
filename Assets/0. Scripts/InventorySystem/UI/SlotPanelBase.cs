using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SlotPanelBase : MonoBehaviour
{
    protected List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    
    public event Action<InventorySlotUI> PointerEnter;
    public event Action PointerExit;
    public event Action<StoredItem, IItemSource> RightClick;
    public event Action<StoredItem, IItemSource, PointerEventData> BeginDrag;
    public event Action<PointerEventData> Dragging;
    public event Action<StoredItem, PointerEventData> EndDrag;

    protected abstract IItemSource GetSource(); // Inventory or EquipInventory


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

    void OnPointerEnter(InventorySlotUI slotUI) => PointerEnter?.Invoke(slotUI);
    void OnPointerExit() => PointerExit?.Invoke();
    public void OnPointerRightClick(StoredItem item) => RightClick?.Invoke(item, GetSource());
    
    void OnBeginDrag(StoredItem item, PointerEventData e)
    {
        PointerExit?.Invoke();
        BeginDrag?.Invoke(item, GetSource(), e);
    }

    void OnDragging(PointerEventData e) => Dragging?.Invoke(e);

    void OnEndDrag(StoredItem item, PointerEventData e) => EndDrag?.Invoke(item, e);
}
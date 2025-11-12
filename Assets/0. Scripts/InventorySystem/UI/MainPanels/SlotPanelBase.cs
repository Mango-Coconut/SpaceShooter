using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SlotPanelBase : MonoBehaviour
{
    protected List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    public virtual bool IsInteractable => isActiveAndEnabled;
    public IEnumerable<InventorySlotUI> Slots => uiSlots;


public event Action<SlotPanelEventArgs> MouseEntered;
public event Action<SlotPanelEventArgs> MouseExited;
public event Action<SlotPanelEventArgs> RightClicked;
public event Action<SlotPanelEventArgs> DragBegan;
public event Action<SlotPanelEventArgs> Dragging;
public event Action<SlotPanelEventArgs> DragEnded;

    protected abstract StorageTarget GetSource(); // Inventory or EquipInventory


    protected virtual void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null) continue;

            // Pointer
            if (slot.pointerHandler != null)
            {
                slot.pointerHandler.PointerEntered += ForwardMouseEnter;
                slot.pointerHandler.PointerExited += ForwardMouseExit;
            }

            // Click
            if (slot.clickHandler != null)
            {
                //slot.clickHandler.LeftClicked += ;
                slot.clickHandler.RightClicked += ForwardRightClick;
            }

            // Drag
            if (slot.dragHandler != null)
            {
                slot.dragHandler.DragBegan += ForwardBeginDrag;
                slot.dragHandler.Dragging += ForwardDragging;
                slot.dragHandler.DragEnded += ForwardDropped;
            }
        }
    }

    protected virtual void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null) continue;

            if (slot.pointerHandler != null)
            {
                slot.pointerHandler.PointerEntered -= ForwardMouseEnter;
                slot.pointerHandler.PointerExited -= ForwardMouseExit;
            }
            if (slot.clickHandler != null)
            {
                //slot.clickHandler.LeftClicked -= ;
                slot.clickHandler.RightClicked -= ForwardRightClick;
            }
            if (slot.dragHandler != null)
            {
                slot.dragHandler.DragBegan -= ForwardBeginDrag;
                slot.dragHandler.Dragging -= ForwardDragging;
                slot.dragHandler.DragEnded -= ForwardDropped;
            }
        }
    }
    void ForwardMouseEnter(StoredItem item, RectTransform rect)
    {
        MouseEntered?.Invoke(new SlotPanelEventArgs(item, GetSource(), rect, null));
    }
    void ForwardMouseExit()
    {
        MouseExited?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
    }
    public void ForwardRightClick(StoredItem item)
    {
        RightClicked?.Invoke(new SlotPanelEventArgs(item, GetSource(), null, null));
    }

    void ForwardBeginDrag(StoredItem item, PointerEventData e)
    {
        MouseExited?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
        DragBegan?.Invoke(new SlotPanelEventArgs(item, GetSource(), null, e));
    }

    void ForwardDragging(PointerEventData e)
    {
        Dragging?.Invoke(new SlotPanelEventArgs(null, GetSource(), null, null));
    }

    void ForwardDropped(StoredItem item, PointerEventData e)
    {
        DragEnded?.Invoke(new SlotPanelEventArgs(item, GetSource(), null, e));
    }
}

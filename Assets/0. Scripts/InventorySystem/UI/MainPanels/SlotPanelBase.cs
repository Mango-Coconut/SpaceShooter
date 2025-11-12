using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SlotPanelBase : MonoBehaviour
{
    protected List<ISlotUI> uiSlots = new List<ISlotUI>();

    public virtual bool IsInteractable => isActiveAndEnabled;
    public IEnumerable<ISlotUI> Slots => uiSlots;


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

        foreach (ISlotUI slot in uiSlots)
        {
            if (slot == null) continue;

            // Pointer
            if (slot.PointerHandler != null)
            {
                slot.PointerHandler.PointerEntered += ForwardMouseEnter;
                slot.PointerHandler.PointerExited += ForwardMouseExit;
            }

            // Click
            if (slot.ClickHandler != null)
            {
                //slot.clickHandler.LeftClicked += ;
                slot.ClickHandler.RightClicked += ForwardRightClick;
            }

            // Drag
            if (slot.DragHandler != null)
            {
                slot.DragHandler.DragBegan += ForwardBeginDrag;
                slot.DragHandler.Dragging += ForwardDragging;
                slot.DragHandler.DragEnded += ForwardDropped;
            }
        }
    }

    protected virtual void UnSubscribeSlotUI()
    {
        foreach (ISlotUI slot in uiSlots)
        {
            if (slot == null) continue;

            if (slot.PointerHandler != null)
            {
                slot.PointerHandler.PointerEntered -= ForwardMouseEnter;
                slot.PointerHandler.PointerExited -= ForwardMouseExit;
            }
            if (slot.ClickHandler != null)
            {
                //slot.clickHandler.LeftClicked -= ;
                slot.ClickHandler.RightClicked -= ForwardRightClick;
            }
            if (slot.DragHandler != null)
            {
                slot.DragHandler.DragBegan -= ForwardBeginDrag;
                slot.DragHandler.Dragging -= ForwardDragging;
                slot.DragHandler.DragEnded -= ForwardDropped;
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

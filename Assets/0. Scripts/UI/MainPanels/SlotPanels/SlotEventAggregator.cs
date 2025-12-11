using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotEventAggregator : MonoBehaviour
{
    [Header("Source Info")]
    [SerializeField] StorageTarget source;   // Inventory / EquipInventory / QuestReward 등

    readonly List<ISlotUI> uiSlots = new List<ISlotUI>();

    public IEnumerable<ISlotUI> Slots => uiSlots;
    public StorageTarget Source => source;

    public event Action<SlotPanelEventArgs> MouseEntered;
    public event Action<SlotPanelEventArgs> MouseExited;
    public event Action<SlotPanelEventArgs> RightClicked;
    public event Action<SlotPanelEventArgs> DragBegan;
    public event Action<SlotPanelEventArgs> Dragging;
    public event Action<SlotPanelEventArgs> DragEnded;

    void Awake()
    {
        BuildSlotList();
    }

    void OnEnable()
    {
        SubscribeSlotUI();
    }

    void OnDisable()
    {
        UnSubscribeSlotUI();
    }

    void BuildSlotList()
    {
        uiSlots.Clear();

        ISlotUI[] found = GetComponentsInChildren<ISlotUI>(true);
        for (int i = 0; i < found.Length; i++)
        {
            ISlotUI slot = found[i];
            if (slot != null && !uiSlots.Contains(slot))
            {
                uiSlots.Add(slot);
            }
        }
    }
    public void RebuildSlots()
    {
        UnSubscribeSlotUI();
        BuildSlotList();
        SubscribeSlotUI();
    }

    void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        for (int i = 0; i < uiSlots.Count; i++)
        {
            ISlotUI slot = uiSlots[i];
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
                // slot.ClickHandler.LeftClicked += ...
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

    void UnSubscribeSlotUI()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            ISlotUI slot = uiSlots[i];
            if (slot == null) continue;

            if (slot.PointerHandler != null)
            {
                slot.PointerHandler.PointerEntered -= ForwardMouseEnter;
                slot.PointerHandler.PointerExited -= ForwardMouseExit;
            }

            if (slot.ClickHandler != null)
            {
                // slot.ClickHandler.LeftClicked -= ...
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
        => MouseEntered?.Invoke(new SlotPanelEventArgs(item, source, rect, null));

    void ForwardMouseExit()
        => MouseExited?.Invoke(new SlotPanelEventArgs(null, source, null, null));

    void ForwardRightClick(StoredItem item)
        => RightClicked?.Invoke(new SlotPanelEventArgs(item, source, null, null));

    void ForwardBeginDrag(StoredItem item, PointerEventData e)
        => DragBegan?.Invoke(new SlotPanelEventArgs(item, source, null, e));

    void ForwardDragging(PointerEventData e)
        => Dragging?.Invoke(new SlotPanelEventArgs(null, source, null, e));

    void ForwardDropped(StoredItem item, PointerEventData e)
        => DragEnded?.Invoke(new SlotPanelEventArgs(item, source, null, e));

    // 이벤트 추적용(Debug.log)
    // void ForwardMouseEnter(StoredItem item, RectTransform rect) { Log.Info($"Slot Mouse Enter"); MouseEntered?.Invoke(new SlotPanelEventArgs(item, source, rect, null)); }
    // void ForwardMouseExit() { Log.Info($"Slot Mouse Exit"); MouseExited?.Invoke(new SlotPanelEventArgs(null, source, null, null)); }
    // void ForwardRightClick(StoredItem item) { Log.Info($"Slot Mouse RightClick"); RightClicked?.Invoke(new SlotPanelEventArgs(item, source, null, null)); }
    // void ForwardBeginDrag(StoredItem item, PointerEventData e) { Log.Info($"Slot Mouse DragBegin"); MouseExited?.Invoke(new SlotPanelEventArgs(null, source, null, null)); DragBegan?.Invoke(new SlotPanelEventArgs(item, source, null, e)); }
    // void ForwardDragging(PointerEventData e) { Log.Info($"Slot Mouse Dragging"); Dragging?.Invoke(new SlotPanelEventArgs(null, source, null, e)); }
    // void ForwardDropped(StoredItem item, PointerEventData e) { Log.Info($"Slot Mouse DragEnd"); DragEnded?.Invoke(new SlotPanelEventArgs(item, source, null, e)); }
}

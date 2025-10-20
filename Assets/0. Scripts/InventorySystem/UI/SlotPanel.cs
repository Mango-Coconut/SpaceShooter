using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanel : MonoBehaviour, ISlotPanel
{
    [SerializeField] Inventory inventory;
    public Inventory Inventory => inventory;
    [SerializeField] GameObject slotPrefab;

    void OnEnable()
    {
        if (inventory == null) return;
        SetInventory(inventory);
        SubscribeInventory();
    }

    void OnDisable()
    {
        UnsubscribeInventory();
        UnSubscribeSlotUI();
    }

    #region 새 인벤토리 불러오기
    readonly List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    public void Clear()
    {
        inventory = null;
    }
    public void SetInventory(Inventory newInventory)
    {
        // 새 인벤토리 세팅
        if (!ReferenceEquals(inventory, newInventory))
        {
            inventory = newInventory;

            // 새로운 인벤토리 이벤트 구독
            SubscribeInventory();
        }

        // 슬롯 세팅
        SetSlot(inventory == null ? 0 : inventory.MaxSlotNum);

        Refresh();
    }

    // 인벤토리 세팅 시 슬롯 재생성
    void SetSlot(int targetCount)
    {
        //이벤트 해제
        UnSubscribeSlotUI();

        // 부족하면 생성
        for (int i = uiSlots.Count; i < targetCount; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, transform).GetComponent<InventorySlotUI>();
            uiSlots.Add(slot);
        }
        // 넘치면 제거
        for (int i = uiSlots.Count - 1; i >= targetCount; i--)
        {
            InventorySlotUI slot = uiSlots[i];
            if (slot != null) Destroy(slot.gameObject);
            uiSlots.RemoveAt(i);
        }

        //이벤트 구독
        SubscribeSlotUI();
    }
    #endregion

    #region UI 갱신
    ItemType categoryFilter = ItemType.All;
    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null)
        {
            NullChecker.NullCheck(this, nameof(inventory));
            return;
        }

        int uiIndex = 0;
        foreach (StoredItem si in inventory.Slots)
        {
            if (categoryFilter == ItemType.All || categoryFilter == si.itemdata.type)
            {
                if (uiIndex < uiSlots.Count)
                {
                    uiSlots[uiIndex].Bind(si);
                }
                uiIndex++;
            }
        }

        // 남은 슬롯은 Clear
        for (int i = uiIndex; i < uiSlots.Count; i++)
        {
            uiSlots[i].Clear();
        }
    }

    #endregion

    #region 인벤토리 이벤트 구독
    void SubscribeInventory()
    {
        if (inventory == null)
        {
            NullChecker.NullCheck(this, nameof(inventory));
            return;
        }

        UnsubscribeInventory();
        inventory.OnChanged += Refresh;
    }

    void UnsubscribeInventory()
    {
        if (inventory == null) return;
        inventory.OnChanged -= Refresh;
    }
    #endregion

    #region 인벤토리 슬롯 UI 이벤트 구독
    //모든 인벤토리 슬롯 UI 이벤트 구독
    void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;

            slot.handler.PointerEnter += HandlePointerEnter;
            slot.handler.PointerExit += HandlePointerExit;
            //slot.handler.LeftClick += 
            //slot.handler.RightClick += 
            slot.handler.BeginDragSlot += HandleBeginDrag;
            slot.handler.DragSlot += HandleDrag;
            slot.handler.EndDragSlot += HandleEndDrag;
        }
    }
    void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;

            slot.handler.PointerEnter -= HandlePointerEnter;
            slot.handler.PointerExit -= HandlePointerExit;
            //slot.handler.LeftClick -= 
            //slot.handler.RightClick -= 
            slot.handler.BeginDragSlot -= HandleBeginDrag;
            slot.handler.DragSlot -= HandleDrag;
            slot.handler.EndDragSlot -= HandleEndDrag;
        }
    }
    #endregion



    #region 이벤트 포워딩(InventoryUI에서 구독)
    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI, IItemSource, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;
    
    void HandlePointerEnter(InventorySlotUI slotUI)
    {
        TooltipShown?.Invoke(slotUI);
    }

    void HandlePointerExit(InventorySlotUI slotUI)
    {
        TooltipHidden?.Invoke(slotUI);
    }

    void HandleBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 시작 시 툴팁 강제 숨김
        TooltipHidden?.Invoke(slotUI);
        BeginDrag?.Invoke(slotUI, inventory, e);
    }

    void HandleDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Dragging?.Invoke(slotUI, e);
    }

    void HandleEndDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Refresh();
        Dropped?.Invoke(slotUI, e);
    }
    #endregion
}
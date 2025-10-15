using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanel : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    public Inventory Inventory => inventory;
    [SerializeField] GameObject slotPrefab;

    ItemType categoryFilter = ItemType.All;
    readonly List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    // ───────────────────────── 이벤트(재발행) ─────────────────────────
    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

    // ───────────────────────── 라이프사이클 ─────────────────────────

    void OnEnable()
    {
        if (inventory == null) return;
        SetInventory(inventory);
        SubscribeInventory();
        WireSlotHandlers(subscribe: true);
    }


    void OnDisable()
    {
        UnsubscribeInventory();
        WireSlotHandlers(subscribe: false);
    }

    // ───────────────────────── 컨테이너 바인딩 ─────────────────────────
    public void SetInventory(Inventory newInventory)
    {
        if(ReferenceEquals(inventory, newInventory))
        {
            EnsureSlotCount(newInventory != null ? newInventory.maxSlotNum : 0);
            Refresh();
            return;
        }
        
        inventory = newInventory;

        // 슬롯 수 맞추기 (늘리기/줄이기 모두 고려)
        EnsureSlotCount(inventory != null ? inventory.maxSlotNum : 0);

        // 새로운 컨테이너 구독
        SubscribeInventory();
        WireSlotHandlers(subscribe: true);

        Refresh();
    }

    void SubscribeInventory()
    {
        if (inventory == null)
        {
            NullChecker.NullCheck(this, nameof(inventory));
            return;
        }
        inventory.Changed -= Refresh;
        inventory.Changed += Refresh;
    }

    void UnsubscribeInventory()
    {
        if (inventory == null)
        {
            //NullChecker.NullCheck(this, nameof(inventory));
            return;
        }
        inventory.Changed -= Refresh;
    }

    // ───────────────────────── 슬롯 핸들러 구독/해제 ─────────────────────────
    void WireSlotHandlers(bool subscribe)
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null)
            {
                //NullChecker.NullCheck(this, nameof(slot));
                return;
            }

            if (subscribe)
            {
                slot.handler.PointerEnter += HandlePointerEnter;
                slot.handler.PointerExit += HandlePointerExit;
                slot.handler.RightClick += UseItem;
                slot.handler.BeginDragSlot += HandleBeginDrag;
                slot.handler.DragSlot += HandleDrag;
                slot.handler.EndDragSlot += HandleEndDrag;
            }
            else
            {
                slot.handler.PointerEnter -= HandlePointerEnter;
                slot.handler.PointerExit -= HandlePointerExit;
                slot.handler.RightClick -= UseItem;
                slot.handler.BeginDragSlot -= HandleBeginDrag;
                slot.handler.DragSlot -= HandleDrag;
                slot.handler.EndDragSlot -= HandleEndDrag;
            }
        }
    }

    void EnsureSlotCount(int targetCount)
    {
        // 부족하면 생성
        for (int i = uiSlots.Count; i < targetCount; i++)
        {
            var child = Instantiate(slotPrefab, transform).GetComponent<InventorySlotUI>();
            uiSlots.Add(child);
        }
        // 넘치면 제거(필요 시). 보통은 남겨두고 Clear만 해도 됨.
        // 아래는 엄밀 모드:
        for (int i = uiSlots.Count - 1; i >= targetCount; i--)
        {
            var s = uiSlots[i];
            // 핸들러 구독되어 있을 수 있으니 안전하게 해제
            if (s != null && s.handler != null)
            {
                s.handler.PointerEnter -= HandlePointerEnter;
                s.handler.PointerExit -= HandlePointerExit;
                s.handler.RightClick -= UseItem;
                s.handler.BeginDragSlot -= HandleBeginDrag;
                s.handler.DragSlot -= HandleDrag;
                s.handler.EndDragSlot -= HandleEndDrag;
            }
            if (s != null) Destroy(s.gameObject);
            uiSlots.RemoveAt(i);
        }
    }

    // ───────────────────────── UI 갱신 ─────────────────────────
    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    void Refresh()
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


    // ───────────────────────── 슬롯 → 패널 재발행 ─────────────────────────
    void HandlePointerEnter(InventorySlotUI slotUI)
    {
        TooltipShown?.Invoke(slotUI);
    }

    void HandlePointerExit(InventorySlotUI slotUI)
    {
        TooltipHidden?.Invoke(slotUI);
    }
    void UseItem(InventorySlotUI slotUI)
    {
        ItemData data = slotUI.EnterItem.itemdata;
        bool isUse = inventory.UseItem(data);
        if (isUse)
        {
            Debug.Log($"{data.name} 1개 사용");
        } 
        else Debug.Log($"사용할 수 없습니다");
        Refresh();
    }

    void HandleBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 시작 시 툴팁 강제 숨김
        TooltipHidden?.Invoke(slotUI);
        BeginDrag?.Invoke(slotUI, e);
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
}
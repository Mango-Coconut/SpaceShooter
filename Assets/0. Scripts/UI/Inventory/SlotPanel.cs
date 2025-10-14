using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanel : MonoBehaviour
{
    [SerializeField] private Container container;
    [SerializeField] private GameObject slotPrefab;

    private ItemType categoryFilter = ItemType.All;
    private readonly List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    // ───────────────────────── 이벤트(재발행) ─────────────────────────
    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

    // ───────────────────────── 라이프사이클 ─────────────────────────

    private void OnEnable()
    {
        if (container == null) return;
        SetContainer(container);
        SubscribeContainer();
        WireSlotHandlers(subscribe: true);
    }


    private void OnDisable()
    {
        UnsubscribeContainer();
        WireSlotHandlers(subscribe: false);
    }

    // ───────────────────────── 컨테이너 바인딩 ─────────────────────────
    public void SetContainer(Container newContainer)
    {
        if(ReferenceEquals(container, newContainer))
        {
            EnsureSlotCount(newContainer != null ? newContainer.maxSlotNum : 0);
            Refresh();
            return;
        }
        
        container = newContainer;

        // 슬롯 수 맞추기 (늘리기/줄이기 모두 고려)
        EnsureSlotCount(container != null ? container.maxSlotNum : 0);

        // 새로운 컨테이너 구독
        SubscribeContainer();
        WireSlotHandlers(subscribe: true);

        Refresh();
    }

    private void SubscribeContainer()
    {
        if (container == null)
        {
            NullChecker.NullCheck(this, nameof(container));
            return;
        }
        container.Changed -= Refresh; // 중복 방지
        container.Changed += Refresh;
    }

    private void UnsubscribeContainer()
    {
        if (container == null)
        {
            //NullChecker.NullCheck(this, nameof(container));
            return;
        }
        container.Changed -= Refresh;
    }

    // ───────────────────────── 슬롯 핸들러 구독/해제 ─────────────────────────
    private void WireSlotHandlers(bool subscribe)
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
                slot.handler.BeginDragSlot += HandleBeginDrag;
                slot.handler.DragSlot += HandleDrag;
                slot.handler.EndDragSlot += HandleEndDrag;
            }
            else
            {
                slot.handler.PointerEnter -= HandlePointerEnter;
                slot.handler.PointerExit -= HandlePointerExit;
                slot.handler.BeginDragSlot -= HandleBeginDrag;
                slot.handler.DragSlot -= HandleDrag;
                slot.handler.EndDragSlot -= HandleEndDrag;
            }
        }
    }

    private void EnsureSlotCount(int targetCount)
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

    private void Refresh()
    {
        if (container == null)
        {
            NullChecker.NullCheck(this, nameof(container));
            return;
        }

        int uiIndex = 0;
        foreach (StoredItem si in container.slots)
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
            // 툴팁이 떠 있었을 수 있으니 상위에서 강제 Hide를 고려해도 좋음
            uiSlots[i].Clear();
        }
    }

    // ───────────────────────── 슬롯 → 패널 재발행 ─────────────────────────
    private void HandlePointerEnter(InventorySlotUI slotUI)
    {
        TooltipShown?.Invoke(slotUI);
    }

    private void HandlePointerExit(InventorySlotUI slotUI)
    {
        TooltipHidden?.Invoke(slotUI);
    }

    private void HandleBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 시작 시 툴팁 강제 숨김 권장
        TooltipHidden?.Invoke(slotUI);
        BeginDrag?.Invoke(slotUI, e);
    }

    private void HandleDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Dragging?.Invoke(slotUI, e);
    }

    private void HandleEndDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Dropped?.Invoke(slotUI, e);
    }
}
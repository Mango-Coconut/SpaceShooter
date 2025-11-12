using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotPanel : SlotPanelBase
{
    [SerializeField] InventoryMono inventory;
    public InventoryMono Inventory => inventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] StorageTarget myStorageType;


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

    #region 인벤토리 세팅 관련

    // 새로운 인벤토리 세팅
    public void SetInventory(InventoryMono newInventory)
    {
        // 새 인벤토리 세팅
        if (!ReferenceEquals(inventory, newInventory))
        {
            inventory = newInventory;

            // 새로운 인벤토리 이벤트 구독
            SubscribeInventory();
        }

        // 슬롯 세팅
        SetSlot(inventory == null ? 0 : inventory.Capacity);

        Refresh();
    }

    // 인벤토리 세팅 시 슬롯UI 재생성
    protected void SetSlot(int targetCount)
    {
        //이벤트 해제
        UnSubscribeSlotUI();

        // 부족하면 생성
        for (int i = uiSlots.Count; i < targetCount; i++)
        {
            ISlotUI slot = Instantiate(slotPrefab, transform).GetComponent<ISlotUI>();
            uiSlots.Add(slot);
        }
        // 넘치면 제거
        for (int i = uiSlots.Count - 1; i >= targetCount; i--)
        {
            ISlotUI slot = uiSlots[i];
            if (slot != null) Destroy(slot.GO);
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
            if (categoryFilter == ItemType.All || categoryFilter == si.itemData.type)
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
        inventory.Core.OnItemChanged += Refresh;
        inventory.Core.OnCoinChanged += ForwardChangedCoin;
    }

    void UnsubscribeInventory()
    {
        if (inventory == null) return;
        inventory.Core.OnItemChanged -= Refresh;
        inventory.Core.OnCoinChanged += ForwardChangedCoin;
    }

    public event Action<int> OnChangedCoin;
    void ForwardChangedCoin(int coin)
    {
        OnChangedCoin?.Invoke(coin);
    }

    protected override StorageTarget GetSource()
    {
        return myStorageType;
    }
    #endregion
}
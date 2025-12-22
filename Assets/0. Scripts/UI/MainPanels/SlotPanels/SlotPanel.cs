using System.Collections.Generic;
using UnityEngine;

public class SlotPanel : MonoBehaviour
{
    public InventoryMono inventory;
    public InventoryMono Inventory => inventory;
    public GameObject slotPrefab;
    [SerializeField] StorageTarget myStorageType;
    [SerializeField] protected CoinPanel coinPanel;
    protected ItemPanelEventAggregator forwarder;
    public ItemPanelEventAggregator Forwarder => forwarder;

    public readonly List<IInteractiveView<StoredItem>> uiSlots = new List<IInteractiveView<StoredItem>>();

    protected void EnsureGetComponent()
    {
        if(forwarder != null) return;
        else forwarder = GetComponent<ItemPanelEventAggregator>();
    }

    void OnEnable()
    {
        OnPanelEnabled();
    }

    void OnDisable()
    {
        OnPanelDisabled();
    }
    protected virtual void OnPanelEnabled()
    {
        if (inventory == null) return;

        SetInventory(inventory); // 같은 인벤이어도 복구 루틴을 타게
        Refresh();
    }

    protected virtual void OnPanelDisabled()
    {
        UnsubscribeInventory();
    }

    #region 인벤토리 세팅 관련

    // 새로운 인벤토리 세팅
    public virtual void SetInventory(InventoryMono newInventory)
    {
        EnsureGetComponent();

        if (ReferenceEquals(inventory, newInventory))
        {
            // 같은 인벤이라도, Enable/Disable로 구독이 끊겼을 수 있으니
            // 여기서 "복구"는 허용하는 게 안전함
            EnsureUI();
            SubscribeInventory(); // 멱등이면 OK
            return;
        }

        UnsubscribeInventory();

        inventory = newInventory;

        EnsureUI();
        SubscribeInventory();
    }

    // UI 상태 재정리
    void EnsureUI()
    {
        if (inventory == null) return;

        int target = inventory.Core.Capacity;

        if (uiSlots.Count != target)
        {
            SetSlot(target);
        }

        forwarder.RebuildViews(uiSlots);
    }

    // 인벤토리 세팅 시 슬롯UI 재생성
    protected void SetSlot(int targetCount)
    {
        uiSlots.Clear();

        // 부족하면 생성
        for (int i = 0; i < targetCount; i++)
        {
            IInteractiveView<StoredItem> slot = null;

            if (i < transform.childCount)
            {
                slot = transform.GetChild(i).GetComponent<IInteractiveView<StoredItem>>();
                if (slot == null)
                {
                    slot = Instantiate(slotPrefab, transform).GetComponent<IInteractiveView<StoredItem>>();
                }
            }
            else
            {
                slot = Instantiate(slotPrefab, transform).GetComponent<IInteractiveView<StoredItem>>();
            }

            if (slot != null)
            {
                uiSlots.Add(slot);
            }
        }

        // 현재 ISlotUI를 가진 자식들만 모음
        List<Transform> slotChildren = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.TryGetComponent<IInteractiveView<StoredItem>>(out _))
                slotChildren.Add(child);
        }

        // ISlotUI가 targetCount보다 많으면 초과분 삭제
        for (int i = slotChildren.Count - 1; i >= targetCount; i--)
        {
            Destroy(slotChildren[i].gameObject);
        }
    }


    #endregion

    #region UI 갱신
    ItemType categoryFilter = ItemType.All;
    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    public virtual void Refresh()
    {
        if (inventory == null)
        {
            NullChecker.NullCheck(this, nameof(inventory));
            return;
        }

        int uiIndex = 0;
        foreach (StoredItem item in inventory.Slots)
        {
            if (categoryFilter == ItemType.All || categoryFilter == item.itemData.type)
            {
                if (uiIndex < uiSlots.Count)
                {
                    uiSlots[uiIndex].Bind(item);
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

    void CoinRefresh(int coin)
    {
        coinPanel.SetCoin(coin);
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

        if(coinPanel == null) return;
        inventory.Core.OnCoinChanged += CoinRefresh;
    }

    void UnsubscribeInventory()
    {
        if (inventory == null) return;
        inventory.Core.OnItemChanged -= Refresh;

        if(coinPanel == null) return;
        inventory.Core.OnCoinChanged -= CoinRefresh;
    }


    public StorageTarget GetSource()
    {
        return myStorageType;
    }
    #endregion
}
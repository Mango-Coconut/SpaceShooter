using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] ShopInventory shopInventory;      // 상점 재고(InventoryMono)
    [SerializeField] InventoryMono playerInventory;    // 플레이어 인벤토리(InventoryMono)

    // 카트: 같은 ItemData 기준으로 스택형 수량 + 유니크 개수(=아이템 개수)
    readonly Dictionary<ItemData, int> cart = new Dictionary<ItemData, int>();

    public int TotalPrice { get; private set; }
    public bool CanPurchase { get; private set; }
    public int AdditionalSlotsNeeded { get; private set; }   // 공간 부족 시 몇 칸 모자라는지

    public event Action<int, bool> OnCartUpdated; // (totalPrice, canPurchase)
    public event Action OnPurchased;

    void OnEnable()
    {
        // 인벤토리/코인/재고 변화에 반응하여 항상 최신 상태로 Recalc
        if (playerInventory != null && playerInventory.Core != null)
        {
            playerInventory.Core.OnItemChanged += Recalc;     // 플레이어 인벤토리 변화
            playerInventory.Core.OnCoinChanged += HandleCoinChanged; // 코인 변화
        }

        // ShopInventory 쪽에 별도 이벤트가 없을 수 있으니, 필요 시 외부에서 Recalc 호출해도 OK
        Recalc();
    }

    void OnDisable()
    {
        if (playerInventory != null && playerInventory.Core != null)
        {
            playerInventory.Core.OnItemChanged -= Recalc;
            playerInventory.Core.OnCoinChanged -= HandleCoinChanged;
        }
    }

    void HandleCoinChanged(int _)
    {
        Recalc();
    }

    // ----- 카트 갱신 API -----

    // 스택형은 n개, 유니크는 0/1 또는 n개(상점 재고가 여러 개면 여러 개 담을 수 있음)
    public void SetQuantity(ItemData item, int quantity)
    {
        if (item == null) return;

        // 재고 상한으로 클램프
        int stock = GetStock(item);
        if (quantity < 0) quantity = 0;
        if (quantity > stock) quantity = stock;

        // 스택형은 그대로, 유니크도 "개수"로 취급(구분 불필요 모드)
        if (quantity == 0)
        {
            if (cart.ContainsKey(item)) cart.Remove(item);
        }
        else
        {
            cart[item] = quantity;
        }

        Recalc();
    }

    public void ClearCart()
    {
        cart.Clear();
        Recalc();
    }

    // ----- 실시간 판단 -----

    public void Recalc()
    {
        // 1) 총액
        int total = 0;
        foreach (KeyValuePair<ItemData, int> kv in cart)
        {
            ItemData data = kv.Key;
            int qty = kv.Value;
            int unitPrice = (data != null) ? data.price : 0; // 가격은 ItemData.price 사용
            total += unitPrice * qty;
        }
        TotalPrice = total;

        // 2) 재고 OK? (상점 재고 기준)
        bool stockOk = true;
        foreach (KeyValuePair<ItemData, int> kv in cart)
        {
            if (kv.Value > GetStock(kv.Key))
            {
                stockOk = false;
                break;
            }
        }

        // 3) 공간 OK? (배치 CanAddItemsBatch)
        bool spaceOk = false;
        int additional = 0;
        if (playerInventory != null && playerInventory.Core != null)
        {
            spaceOk = playerInventory.Core.CanAddItemsBatch(cart, out additional);
        }
        AdditionalSlotsNeeded = spaceOk ? 0 : additional;

        // 4) 코인 OK?
        int coin = (playerInventory != null && playerInventory.Core != null) ? playerInventory.Core.MyCoin : 0;
        bool coinOk = coin >= TotalPrice;

        CanPurchase = (TotalPrice > 0) && stockOk && spaceOk && coinOk;

        if (OnCartUpdated != null)
        {
            OnCartUpdated.Invoke(TotalPrice, CanPurchase);
        }
    }

    // ----- 구매 확정 -----

    public bool Purchase()
    {
        if (!CanPurchase) return false;

        // 경합 대비 재검증
        Recalc();
        if (!CanPurchase) return false;

        if (playerInventory == null || playerInventory.Core == null) return false;
        if (shopInventory == null || shopInventory.Core == null) return false;

        // 1) 공간 재확인 (보수적으로 한 번 더)
        int additional;
        if (!playerInventory.Core.CanAddItemsBatch(cart, out additional))
        {
            Recalc();
            return false;
        }

        // 2) 실제 지급/차감
        //    - 스택형: 이 시점에서만 StoredItem 생성 → TryAddItem
        //    - 유니크: 상점의 유니크 인스턴스 하나씩 찾아서 제거 → 플레이어에 TryAdd
        foreach (KeyValuePair<ItemData, int> kv in cart)
        {
            ItemData data = kv.Key;
            int qty = kv.Value;
            if (data == null || qty <= 0) continue;

            if (IsStackable(data))
            {
                StoredItem stack = new StoredItem(data, qty);
                bool okAdd = playerInventory.Core.TryAddItem(stack);
                if (!okAdd)
                {
                    Recalc();
                    return false;
                }

                // 상점 재고 차감(스택형)
                bool okConsume = shopInventory.Core.TryRemoveItem(data, qty);
                if (!okConsume)
                {
                    // 롤백 시도: 플레이어에서 제거 시도
                    playerInventory.Core.TryRemoveItem(data, qty);
                    Recalc();
                    return false;
                }
            }
            else
            {
                // 유니크: 개수만큼 상점에서 실제 인스턴스를 하나씩 찾아 제거 후 플레이어에게 지급
                for (int i = 0; i < qty; i++)
                {
                    StoredItem unique = PopOneUniqueFromShop(data);
                    if (unique == null)
                    {
                        Recalc();
                        return false;
                    }

                    bool okAdd = playerInventory.Core.TryAddItem(unique);
                    if (!okAdd)
                    {
                        // 롤백: 상점으로 되돌리기
                        shopInventory.Core.TryAddItem(unique);
                        Recalc();
                        return false;
                    }
                }
            }
        }

        // 3) 코인 차감 (마지막에)
        playerInventory.Core.ModifyCoin(-TotalPrice);

        // 4) 카트 비우고 갱신
        cart.Clear();
        Recalc();

        if (OnPurchased != null) OnPurchased.Invoke();
        return true;
    }

    // ----- 헬퍼 -----

    static bool IsStackable(ItemData data)
    {
        return data != null && data.maxStack > 1;
    }

    // 상점 재고 개수(스택형 총합 + 유니크 개수)
    int GetStock(ItemData data)
    {
        if (shopInventory == null || shopInventory.Core == null || data == null) return 0;

        IReadOnlyList<StoredItem> slots = shopInventory.Core.Slots;
        int count = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s == null || s.itemData != data) continue;

            if (s.IsUniqueInstance())
            {
                // 유니크는 개별 1개씩 취급
                count += 1;
            }
            else
            {
                // 스택형 총합
                count += s.count;
            }
        }

        return count;
    }

    // 상점에서 특정 ItemData의 유니크 인스턴스 하나를 찾아 제거하여 반환
    StoredItem PopOneUniqueFromShop(ItemData data)
    {
        if (shopInventory == null || shopInventory.Core == null || data == null) return null;

        IReadOnlyList<StoredItem> slots = shopInventory.Core.Slots;

        // 뒤에서 앞으로 순회하며 첫 유니크 인스턴스를 제거
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            StoredItem s = slots[i];
            if (s == null || s.itemData != data) continue;
            if (!s.IsUniqueInstance()) continue;

            // 이 StoredItem은 실제 유니크 인스턴스(고유 ID)
            bool removed = shopInventory.Core.TryRemoveItem(s);
            if (!removed) continue;

            return s; // 소유권 이전용으로 반환
        }

        return null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class ShopSlotPanel : SlotPanel
{
    int playerCoin;
    public event Action<StoredItem, int> BoughtItem;

    protected override void OnPanelEnabled()
    {
        base.OnPanelEnabled();
    }

    protected override void OnPanelDisabled()
    {
        UnsubscribeShopSlots();
        slotByProductId.Clear();
        activeProductIds.Clear();
        base.OnPanelDisabled();
    }

    // 기존 슬롯의 MouseEvent 포워딩은 forwarder에서 알아서 처리
    // 여기는 ShopSlot만의 이벤트를 추가로 포워딩
    public void SubscribeShopSlots()
    {
        UnsubscribeShopSlots();
        foreach (IInteractiveView<StoredItem> slot in uiSlots)
        {
            ShopSlot shopSlot = slot as ShopSlot;
            if (shopSlot == null) continue;

            shopSlot.BoughtItem += ForwardBoughtItem;
        }
    }

    void UnsubscribeShopSlots()
    {
        foreach (IInteractiveView<StoredItem> slot in uiSlots)
        {
            ShopSlot shopSlot = slot as ShopSlot;
            if (shopSlot == null) continue;

            shopSlot.BoughtItem -= ForwardBoughtItem;
        }
    }

    public override void SetInventory(InventoryMono newInventory)
    {
        base.SetInventory(newInventory);
        SubscribeShopSlots();
    }

    // 인벤토리는 매번 슬롯이 바뀌어도 상관 없지만,   (물론 기획의도에 따라 자동 정렬 안 되게 한다면 다르겠지만)
    // 상점은 품목과 슬롯이 1:1 매칭 되어야 함. (품목 일부가 사라져도 슬롯에 따라가야 함)
    // 때문에 base.Refresh(); 사용하지 않고 따로 구현
    readonly Dictionary<string, ShopSlot> slotByProductId = new Dictionary<string, ShopSlot>();
    readonly HashSet<string> activeProductIds = new HashSet<string>();
    public override void Refresh()
    {
        if (inventory == null) return;

        activeProductIds.Clear();

        // 상점 재고를 모두 확인
        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            StoredItem item = inventory.Slots[i];
            if (item == null || item.itemData == null) continue;

            string productId = item.itemData.id; // ItemData.id :contentReference[oaicite:0]{index=0}
            if (string.IsNullOrEmpty(productId)) continue;

            activeProductIds.Add(productId);

            // 매핑이 안 되어 있으면 빈 슬롯을 찾아서 매핑 시킴
            ShopSlot slot;
            if (!slotByProductId.TryGetValue(productId, out slot) || slot == null)
            {
                slot = FindUnmappedSlot(); // "매핑 안 된 슬롯"만 반환하도록
                if (slot == null) break;

                slotByProductId[productId] = slot;
            }

            slot.SetPlayerCoin(playerCoin);
            slot.Bind(item);
        }

        // 품절된 상품의 슬롯은 Clear 시키기
        List<string> removeKeys = null;
        foreach (KeyValuePair<string, ShopSlot> kv in slotByProductId)
        {
            if (!activeProductIds.Contains(kv.Key))
            {
                if (kv.Value != null) { kv.Value.Clear(); }

                if (removeKeys == null) { removeKeys = new List<string>(); }
                removeKeys.Add(kv.Key);
            }
        }

        if (removeKeys != null)
        {
            for (int i = 0; i < removeKeys.Count; i++)
            {
                slotByProductId.Remove(removeKeys[i]);
            }
        }
    }

    ShopSlot FindUnmappedSlot()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            ShopSlot slot = uiSlots[i] as ShopSlot;
            if (slot == null) continue;

            // "현재 어떤 productId에도 안 물린 슬롯"만
            bool mapped = false;
            foreach (KeyValuePair<string, ShopSlot> kv in slotByProductId)
            {
                if (ReferenceEquals(kv.Value, slot))
                {
                    mapped = true;
                    break;
                }
            }

            if (!mapped) { return slot; }
        }

        return null;
    }

    public void SetCoin(int coin)
    {
        playerCoin = coin;

        foreach (KeyValuePair<string, ShopSlot> kv in slotByProductId)
        {
            if (kv.Value == null) continue;
            kv.Value.SetPlayerCoin(playerCoin);
            kv.Value.Refresh();
        }
    }

    public void ForwardBoughtItem(StoredItem item, int amount)
    {
        BoughtItem?.Invoke(item, amount);
    }
}
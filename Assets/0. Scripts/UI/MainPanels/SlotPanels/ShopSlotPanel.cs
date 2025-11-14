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
        SubscribeShopSlots();
    }
    protected override void OnPanelDisabled()
    {
        UnsubscribeShopSlots();
        base.OnPanelDisabled();
    }

    // 기존 슬롯의 MouseEvent 포워딩은 SlotPanelBase에서
    // 여기는 ShopSlot만의 이벤트를 추가로 포워딩
    void SubscribeShopSlots()
    {
        UnsubscribeShopSlots();
        for (int i = 0; i < uiSlots.Count; i++)
        {
            ShopSlot shopSlot = uiSlots[i] as ShopSlot;
            if (shopSlot == null) continue;
            shopSlot.BoughtItem += ForwardBoughtItem;
        }
    }

    void UnsubscribeShopSlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            ShopSlot shopSlot = uiSlots[i] as ShopSlot;
            if (shopSlot == null) continue;
            shopSlot.BoughtItem -= ForwardBoughtItem;
        }
    }

    public override void Refresh()
    {
        base.Refresh();  // 기본 인벤토리 → 슬롯 바인딩

        // 추가로 각 슬롯에 코인 정보 넘기기
        for (int i = 0; i < uiSlots.Count; i++)
        {
            ShopSlot shopSlot = uiSlots[i] as ShopSlot;
            if (shopSlot != null)
            {
                shopSlot.SetPlayerCoin(playerCoin);
            }
        }
    }
    public void SetCoin(int coin)
    {
        playerCoin = coin;
        Refresh();
    }

    public void ForwardBoughtItem(StoredItem item, int amount)
    {
        BoughtItem?.Invoke(item, amount);
    }
}
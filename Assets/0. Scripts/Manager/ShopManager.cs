using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] ShopSlotPanel shopSlotPanel;
    [SerializeField] InventoryMono playerInventory;

    void OnEnable()
    {
        shopSlotPanel.BoughtItem -= TryBuyItem;
        shopSlotPanel.BoughtItem += TryBuyItem;
    }
    void OnDisable()
    {
        shopSlotPanel.BoughtItem -= TryBuyItem;
    }

    void TryBuyItem(StoredItem item, int amount)
    {
        if (item == null || item.itemData == null || amount <= 0) return;
        if (playerInventory == null || playerInventory.Core == null)
        {
            Log.Error("TryBuyItem failed: PlayerInventoryMono or Core is null.");
            return;
        }
        int totalPrice = item.itemData.price * amount;

        if(playerInventory.Core.MyCoin < totalPrice)
        {
            // ShopSlot에서 아예 버튼이 안 눌리게 1차로 막지만, 여기서 2차로 방지
            Log.Info($"Coin이 부족합니다.");
            return;
        }
        if(!InventoryManager.Instance.TryBuyItem(item, amount))
        {
            Log.Info($"구매할 수 없습니다.");
            return;
        }
        playerInventory.Core.ModifyCoin(-totalPrice);
        shopSlotPanel.SetCoin(playerInventory.Core.MyCoin);
    }
}

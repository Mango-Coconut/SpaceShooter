using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    SlotPanel shopSlotPanel;
    CoinPanel coinPanel;

    [SerializeField] TMP_Text totalCoinText;
    [SerializeField] Button buyButton;


    void Awake()
    {
        shopSlotPanel = GetComponentInChildren<SlotPanel>();
        coinPanel = GetComponentInChildren<CoinPanel>();
    }

    public void Bind(ShopInventory inventory, int playerCoin)
    {
        shopSlotPanel.SetInventory(inventory);
        coinPanel.SetCoin(inventory.Core.MyCoin);
    }
    
    public void Close()
    {
        
    }
}

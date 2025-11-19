using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinPanel : MonoBehaviour
{
    // Player Inventory 전용. 추후에 Npc Shop 등 런타임 바인딩 추가할거면 리팩토링하기
    [SerializeField] InventoryMono inventory;
    [SerializeField] TMP_Text coinText;

    void Awake()
    {
        if(inventory == null || inventory.Core == null) return;
        inventory.Core.OnCoinChanged -= SetCoin;
        inventory.Core.OnCoinChanged += SetCoin;
        SetCoin(inventory.Core.MyCoin);
    }
    void OnDestroy()
    {
        if(inventory == null || inventory.Core == null) return;
        inventory.Core.OnCoinChanged -= SetCoin;
        
    }

    public void SetCoin(int coin)
    {
        coinText.text = coin.ToString();
    }
}

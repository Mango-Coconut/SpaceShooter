using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinPanel : MonoBehaviour
{
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

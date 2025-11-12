using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    ShopSlotPanel shopSlotPanel;
    CoinPanel coinPanel;

    void Awake()
    {
        shopSlotPanel = GetComponentInChildren<ShopSlotPanel>();
        coinPanel = GetComponentInChildren<CoinPanel>();
    }
}

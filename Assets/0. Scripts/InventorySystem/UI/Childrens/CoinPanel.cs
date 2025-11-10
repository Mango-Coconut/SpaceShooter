using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinPanel : MonoBehaviour
{
    TMP_Text coinText;

    void Awake()
    {
        coinText = GetComponentInChildren<TMP_Text>();
    }
    
    public void SetCoin(int coin)
    {
        coinText.text = coin.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    // 통계용
    public int useCount;         // 얼마나 많이 사용했는지
    public System.DateTime lastUsed; // 최근 사용 시간

    public InventorySlot(ItemData item, int count = 1)
    {
        this.item = item;
        this.count = count;
        this.useCount = 0;
        this.lastUsed = System.DateTime.MinValue;
    }
}

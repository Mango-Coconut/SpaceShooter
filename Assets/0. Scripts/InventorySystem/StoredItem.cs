using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoredItem
{
    public string instanceId;
    public ItemData itemData;
    public int count = 1;

    // 장비 유니크 상태
    // public int enhancement;
    // public float durability
    // 등등 ...

    // // 통계용
    // public int useCount;         // 얼마나 많이 사용했는지
    // public DateTime lastGet;   // ← 획득 시각
    // public DateTime lastUsed;  // ← 사용 시각

    public StoredItem(ItemData itemdata, int count = 1)
    {
        this.itemData = itemdata;
        this.count = count;
        this.instanceId = Guid.NewGuid().ToString("N");
        //this.lastGet = DateTime.UtcNow;
    }
    
    // “이 아이템은 다른 스택과 합칠 수 있는가?” 판단
    public bool IsMergeableWith(StoredItem other)
    {
        if (other == null) return false;
        if (other.itemData != itemData) return false;

        // 장비/유니크 조건: 강화/내구/파츠 등 상태가 있으면 합치지 않음
        bool selfUnique  = IsUniqueInstance();
        bool otherUnique = other.IsUniqueInstance();
        if (selfUnique || otherUnique) return false;

        // 같은 데이터 + 유니크 아님 → 합칠 수 있음
        return true;
    }

    public bool IsUniqueInstance()
    {
        // 정책: maxStack==1 이거나, 상태가 붙은 경우 유니크
        if (itemData != null && itemData.maxStack <= 1) return true;
        //if (enhancement != 0) return true;
        return false;
    }
}
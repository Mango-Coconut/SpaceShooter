using System;
using UnityEngine;

public class Inventory : Container
{
    public event Action OnInventoryChanged;

    private void OnEnable()
    {
        Changed += Forward;
    }

    private void OnDisable()
    {
        Changed -= Forward;
    }

    private void Forward()
    {
        Action handler = OnInventoryChanged;
        if (handler != null) handler.Invoke();
    }

    // 사용 로직: 기록 + 차감
    public bool UseItem(ItemData data, int useCount = 1)
    {
        StoredItem slot = FindSlot(data);

        if (slot == null) return false;
        // 보유한 아이템이 충분하지 않습니다!
        if (slot.count < useCount) return false;

        // (아이템 고유의 사용 로직이 있다면 여기서 호출)
        // 예: data.Use(this);

        slot.useCount++;
        slot.lastUsed = DateTime.Now;

        return TryRemoveItem(slot.itemdata, slot.count);
    }
}
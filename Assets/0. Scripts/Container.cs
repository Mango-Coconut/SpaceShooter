using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container : MonoBehaviour
{
    // 가능하면 외부 수정 막고 읽기만 노출
    public List<StoredItem> slots = new List<StoredItem>();
    public int maxSlotNum = 10;

    public event Action Changed;

    protected void RaiseChanged()
    {
        Changed?.Invoke();
    }

    // 🔎 공통 조회
    protected StoredItem FindSlot(ItemData data)
    {
        return slots.Find(s => s.itemdata == data); // (허용) 컬렉션 람다
    }

    public int CountOf(ItemData data)
    {
        StoredItem slot = FindSlot(data);
        return slot != null ? slot.count : 0;
    }

    public bool HasItem(ItemData data, int amount = 1)
    {
        return CountOf(data) >= amount;
    }

    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (slots.Count >= maxSlotNum && FindSlot(data) == null) return false;

        StoredItem slot = FindSlot(data);
        if (slot != null)
        {
            slot.count += amount;
            slot.lastGet = DateTime.Now;
        }
        else
        {
            StoredItem newSlot = new StoredItem(data, amount);
            newSlot.lastGet = DateTime.Now;
            slots.Add(newSlot);
        }

        RaiseChanged();
        return true;
    }

    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        StoredItem slot = FindSlot(data);
        return TryRemoveItem(slot, amount);
    }

    protected bool TryRemoveItem(StoredItem slot, int amount = 1)
    {
        if (slot == null) return false;
        //제거할 아이템이 충분하지 않습니다!
        if (slot.count < amount) return false;

        slot.count -= amount;
        if (slot.count == 0)
        {
            slots.Remove(slot);
        }

        RaiseChanged();
        return true;
    }

}
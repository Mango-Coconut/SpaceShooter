using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    List<StoredItem> slots = new List<StoredItem>();
    public List<StoredItem> Slots => slots;
    public int maxSlotNum = 10;

    public event Action Changed;

    protected void RaiseChanged()
    {
        Changed?.Invoke();
    }

    protected StoredItem FindSlot(ItemData data)
    {
        return slots.Find(s => s.itemdata == data);
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

        return TryRemoveItem(slot.itemdata, 1);
    }

    public void Sort(int index)
    {
        SortType key = (SortType)index;

        Comparison<StoredItem> comparison = (a, b) =>
        {
            switch (key)
            {
                case SortType.Rarity:   return b.itemdata.rarity.CompareTo(a.itemdata.rarity);
                case SortType.Price:    return b.itemdata.price.CompareTo(a.itemdata.price);
                case SortType.Weight:   return b.itemdata.weight.CompareTo(a.itemdata.weight);
                case SortType.Volume:   return b.itemdata.volume.CompareTo(a.itemdata.volume);
                case SortType.LastGet:  return b.lastGet.CompareTo(a.lastGet);
                case SortType.LastUsed: return b.lastUsed.CompareTo(a.lastUsed);
                case SortType.UseCount: return b.useCount.CompareTo(a.useCount);
                default:                return 0;
            }
        };

        Slots.Sort(comparison);
        RaiseChanged();
    }
}
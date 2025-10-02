using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<StoredItem> slots = new List<StoredItem>();
    public int maxSlotNum = 10;

    public event Action Changed;
    protected void RaiseChanged() => Changed?.Invoke();

    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (slots.Count >= maxSlotNum) return false;

        var slot = slots.Find(s => s.itemdata == data);
        if (slot != null) slot.count += amount;
        else slots.Add(new StoredItem(data, amount));

        RaiseChanged();
        return true;
    }

    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        var slot = slots.Find(s => s.itemdata == data);
        if (slot == null || slot.count < amount) return false;

        slot.count -= amount;
        if (slot.count == 0) slots.Remove(slot);

        RaiseChanged();
        return true;
    }

    public List<StoredItem> GetByCategory(ItemType type)
        => slots.FindAll(s => s.itemdata.type == type);

    public void Sort(int index)
    {
        switch ((SortType)index)
        {
            case SortType.Rarity:   slots = slots.OrderByDescending(s => s.itemdata.rarity).ToList(); break;
            case SortType.Price:    slots = slots.OrderByDescending(s => s.itemdata.price).ToList();  break;
            case SortType.Weight:   slots = slots.OrderByDescending(s => s.itemdata.weight).ToList(); break;
            case SortType.Volume:   slots = slots.OrderByDescending(s => s.itemdata.volume).ToList(); break;
            case SortType.LastGet:  slots = slots.OrderByDescending(s => s.lastGet).ToList();         break;
            case SortType.LastUsed: slots = slots.OrderByDescending(s => s.lastUsed).ToList();        break;
            case SortType.UseCount: slots = slots.OrderByDescending(s => s.useCount).ToList();        break;
        }
        RaiseChanged();
    }
}
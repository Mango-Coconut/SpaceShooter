using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<StoredItem> slots = new List<StoredItem>();
    public event System.Action OnInventoryChanged;
    public void AddItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.itemdata == data);
        if (slot != null) slot.count += amount;
        else slots.Add(new StoredItem(data, amount));

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.itemdata == data);
        if (slot == null) return;

        slot.count -= amount;
        if (slot.count <= 0) slots.Remove(slot);

        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.itemdata == data);
        return slot != null && slot.count >= amount;
    }

    public void UseItem(ItemData data)
    {
        StoredItem slot = slots.Find(s => s.itemdata == data);
        if (slot == null) return;

        // 사용 처리
        slot.count--;
        slot.useCount++;
        slot.lastUsed = System.DateTime.Now;

        if (slot.count <= 0) slots.Remove(slot);

        OnInventoryChanged?.Invoke();
    }

    public List<StoredItem> GetByCategory(ItemType type)
    {
        return slots.FindAll(s => s.itemdata.type == type);
    }

    public void Sort(int index)
    {
        SortType sortType = (SortType)index;
        switch (sortType)
        {
            case SortType.Rarity:
                slots = slots.OrderByDescending(s => s.itemdata.rarity).ToList();
                break;
            case SortType.Price:
                slots = slots.OrderByDescending(s => s.itemdata.price).ToList();
                break;
            case SortType.Weight:
                slots = slots.OrderByDescending(s => s.itemdata.weight).ToList();
                break;
            case SortType.Volume:
                slots = slots.OrderByDescending(s => s.itemdata.volume).ToList();
                break;
            case SortType.LastGet:
                slots = slots.OrderByDescending(s => s.lastGet).ToList();
                break;
            case SortType.LastUsed:
                slots = slots.OrderByDescending(s => s.lastUsed).ToList();
                break;
            case SortType.UseCount:
                slots = slots.OrderByDescending(s => s.useCount).ToList();
                break;
        }
        OnInventoryChanged?.Invoke();
    }
}

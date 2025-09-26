using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum SortType { Rarity, Price, Weight, Volume, LastGet, LastUsed, UseCount }

public class Inventory : MonoBehaviour
{
    public List<StoredItem> slots = new List<StoredItem>();


    public void PrintSlots()
    {
        Debug.Log("items---------------------");
        foreach (var item in slots)
        {
            Debug.Log($"{item.item.name}, {item.count}");
        }
    }
    // 아이템 추가
    public void AddItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.item == data);
        if (slot != null) slot.count += amount;
        else slots.Add(new StoredItem(data, amount));
        PrintSlots();
    }

    // 아이템 제거
    public void RemoveItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.item == data);
        if (slot == null) return;

        slot.count -= amount;
        if (slot.count <= 0) slots.Remove(slot);
    }

    // 아이템 보유 여부 확인
    public bool HasItem(ItemData data, int amount = 1)
    {
        StoredItem slot = slots.Find(s => s.item == data);
        return slot != null && slot.count >= amount;
    }

    public void UseItem(ItemData data)
    {
        StoredItem slot = slots.Find(s => s.item == data);
        if (slot == null) return;

        // 사용 처리
        slot.count--;
        slot.useCount++;
        slot.lastUsed = System.DateTime.Now;

        if (slot.count <= 0) slots.Remove(slot);
    }

    public List<StoredItem> GetByCategory(ItemType type)
    {
        return slots.FindAll(s => s.item.type == type);
    }

    public void Sort(int  index)
    {
        SortType sortType = (SortType)index;
        switch (sortType)
        {
            case SortType.Rarity:
                slots = slots.OrderByDescending(s => s.item.rarity).ToList();
                break;
            case SortType.Price:
                slots = slots.OrderByDescending(s => s.item.price).ToList();
                break;
            case SortType.Weight:
                slots = slots.OrderByDescending(s => s.item.weight).ToList();
                break;
            case SortType.Volume:
                slots = slots.OrderByDescending(s => s.item.volume).ToList();
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
    }
}

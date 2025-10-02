using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<StoredItem> slots = new List<StoredItem>();
    public int maxSlotNum = 10;
    public event System.Action OnInventoryChanged;
    public bool TryAddItem(ItemData data, int amount = 1)
    {
        //아이템 칸 가득 차서 더 이상 넣을 수 없음
        if (slots.Count >= maxSlotNum) return false;

        //이미 해당 아이템이 있으면 count +
        StoredItem slot = slots.Find(s => s.itemdata == data);
        if (slot != null)
        {
            slot.count += amount;
        }
        //없으면 새 슬롯에 넣기
        else
        {
            slots.Add(new StoredItem(data, amount));
        }

        OnInventoryChanged?.Invoke();
        return true;
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

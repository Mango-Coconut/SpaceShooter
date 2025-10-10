using System;
using System.Collections.Generic;

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
    
    public List<StoredItem> GetByCategory(ItemType type)
    {
        return slots.FindAll(s => s.itemdata.type == type);
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

        slots.Sort(comparison);
        RaiseChanged();
    }
}
using System;
using System.Collections.Generic;

public class EquipInventoryCore : IItemSink, IItemSource, ISwapSink
{
    readonly Dictionary<EquipType, StoredItem> equipped = new Dictionary<EquipType, StoredItem>();
    public IReadOnlyDictionary<EquipType, StoredItem> Equipped => equipped;
    public bool TryGetEquipped(EquipType slot, out StoredItem item)
    {
        return equipped.TryGetValue(slot, out item);
    }
    
    public event Action OnChanged;
    void RaiseChanged()
    {
        OnChanged?.Invoke();
    }

    #region  Item Add, Remove, Swap
    public bool CanAddItem(StoredItem item)
    {
        if (item == null) return false;
        if (item.itemData.equiptype == EquipType.None) return false;
        return true;
    }

    public bool TryAddItem(StoredItem item)
    {
        if (!CanAddItem(item)) return false;

        //해당 타입의 장비가 비어있을 경우만 장착 가능
        var slot = item.itemData.equiptype;
        if (equipped.ContainsKey(slot)) return false;

        equipped[slot] = item;
        RaiseChanged();
        return true;
    }

    public bool CanRemoveItem(StoredItem item)
    {
        if (item == null) return false;
        var slot = item.itemData.equiptype;
        return equipped.TryGetValue(slot, out var equippedItem) && equippedItem == item;
    }

    public bool TryRemoveItem(StoredItem item)
    {
        if (!CanRemoveItem(item)) return false;

        var slot = item.itemData.equiptype;
        equipped.Remove(slot);

        RaiseChanged();
        return true;
    }

    public bool CanAddItemSwap(StoredItem newItem, out StoredItem swappedOut)
    {
        swappedOut = null;
        if (!CanAddItem(newItem)) return false;

        var slot = newItem.itemData.equiptype;

        equipped.TryGetValue(slot, out swappedOut);
        return true;
    }
    public bool TryAddItemSwap(StoredItem newItem, out StoredItem swappedOut)
    {
        swappedOut = null;
        if (!CanAddItem(newItem)) return false;

        var slot = newItem.itemData.equiptype;

        // 슬롯에 이미 뭐가 있으면 그걸 뽑아냄
        equipped.TryGetValue(slot, out swappedOut);

        equipped[slot] = newItem;
        RaiseChanged();
        return true;
    }
    #endregion
}

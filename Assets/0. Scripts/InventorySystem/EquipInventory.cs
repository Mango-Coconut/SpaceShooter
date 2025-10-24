using System;
using UnityEngine;

public class EquipInventory : MonoBehaviour, IItemSink, IItemSource, ISwapSink
{
    StoredItem weapon = null;
    public StoredItem Weapon => weapon;
    //TODO 방어구도 구현
    //StoredItem helmet;
    //StoredItem chestArmor;

    public event Action OnChanged;

    public bool CanAddItem(StoredItem item)
    {
        if (item == null || item.itemData == null) return false;
        return item.itemData.type == ItemType.Weapon;
    }

    public bool TryAddItem(StoredItem item)
    {
        if (!CanAddItem(item)) return false;
        if (Weapon != null) return false; // 비어 있을 때만 허용
        weapon = item;
        OnChanged?.Invoke();
        return true;
    }

    public bool CanRemoveItem(StoredItem item)
    {
        return Weapon != null && ReferenceEquals(Weapon, item);
    }

    public bool TryRemoveItem(StoredItem item)
    {
        if (!CanRemoveItem(item)) return false;
        var removed = Weapon;
        weapon = null;
        OnChanged?.Invoke();
        return true;
    }

    public bool CanAddItemSwap(StoredItem item)
    {
        return CanAddItem(item);
    }
    public bool CanAddItemSwap(StoredItem item, out StoredItem willBeSwapped)
    {
        willBeSwapped = null;
        if (!CanAddItem(item)) return false;

        // 동일 인스턴스면 변화 없음
        if (ReferenceEquals(Weapon, item)) return true;

        // 현재 무기가 있다면 그게 튀어나올 예정
        willBeSwapped = Weapon;
        return true;
        
    }

    public bool TryAddItemSwap(StoredItem item, out StoredItem swapped)
    {
        swapped = null;
        if (!CanAddItem(item)) return false;

        if (ReferenceEquals(Weapon, item)) return true; // 변화 없음

        // 현재 무기가 있다면 내보내기
        swapped = Weapon;
        weapon = item;
        OnChanged?.Invoke();
        return true;
    }
}

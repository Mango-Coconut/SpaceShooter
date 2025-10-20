using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipInventory : MonoBehaviour, IStorable
{
    public StoredItem Weapon = null;
    //TODO 방어구도 구현
    //StoredItem helmet;
    //StoredItem chestArmor;

    public bool TryAddItem(StoredItem item)
    {
        if (item == null)
        {
            Debug.Log($"equipInventory -> item null");
            return false;
        }

        switch (item.itemdata.type)
        {
            case ItemType.Weapon:
                if (Weapon != null && Weapon == item)
                {
                    return false;
                }
                if (Weapon != null && Weapon.itemdata != null)
                {
                    if(TryRemoveItem(Weapon) == false)
                    {
                        return false;
                    }
                }
                Weapon = item;
                OnEquipped?.Invoke(Weapon);
                OnWeaponChanged?.Invoke();
                return true;
            // TODO
            // case ItemType.Helmet : ...

            //장비가 아닌 건 받지 않음
            default:
                return false;
        }
    }
    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        return TryAddItem(new StoredItem(data));
    }
    public bool TryRemoveItem(StoredItem item)
    {
        if (item == null) return false;

        switch (item.itemdata.type)
        {
            case ItemType.Weapon:
                if (Weapon == null || Weapon.itemdata == null) return false;

                OnUnequipped?.Invoke(Weapon); //벗은 장비 전달
                Weapon = null; //장비 비우기
                OnWeaponChanged?.Invoke(); //Refresh
                
                return true;
            // case ItemType.Armor:
            //     if (Armor != null && Armor.itemdata == data) { ... }

            default:
                return false;
        }
    }
    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        return TryRemoveItem(new StoredItem(data));
    }

    //장착한 아이템 반환(PlayerController.PlyerWeapon에 전달)
    public event Action<StoredItem> OnEquipped;
    //벗은 아이템 반환(EquipSlotPanel -> PanelManager로 전달)
    public event Action<StoredItem> OnUnequipped;
    //refresh(EquipSlotPanel이 구독)
    public event Action OnWeaponChanged;
}

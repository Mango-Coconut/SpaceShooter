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

    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;

        switch (data.type)
        {
            case ItemType.Weapon:
                // 이미 같은 무기면 아무 것도 안 함
                if (Weapon != null && Weapon.itemdata == data)
                {
                    return true;
                }

                // 기존 무기 제거 (있을 경우)
                if (Weapon != null)
                {
                    TryRemoveItem(Weapon.itemdata);
                }
                // 새 무기 장착
                Weapon = new StoredItem(data, 1);
                Debug.Log($"EquipInventory에 {data.name}을 장착");
                OnEquipped?.Invoke(Weapon);
                return true;

            // TODO: 방어구나 액세서리 같은 타입 추가
            // case ItemType.Armor:
            //     if (Armor == null) { ... }

            default:
                // 장비창은 장비 이외의 아이템은 받지 않음
                return false;
        }
    }


    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;

        switch (data.type)
        {
            case ItemType.Weapon:
                if (Weapon == null) return false;
                if (Weapon.itemdata != data) return false;

                StoredItem unequipped = Weapon;
                Weapon = null;
                OnUnequipped?.Invoke(unequipped);
                Debug.Log($"EquipInventory에 {data.name}을 제거");
                return true;

            // case ItemType.Armor:
            //     if (Armor != null && Armor.itemdata == data) { ... }

            default:
                return false;
        }
    }

    //장착한 아이템 반환(PlayerController에서 필요)
    public event Action<StoredItem> OnEquipped;
    //벗은 아이템 반환(Inventory에서 필요)
    public event Action<StoredItem> OnUnequipped;
}

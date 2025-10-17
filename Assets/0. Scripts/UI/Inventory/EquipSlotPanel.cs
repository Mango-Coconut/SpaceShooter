using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSlotPanel : MonoBehaviour
{
    [SerializeField] EquipInventory equipInventory;
    public EquipInventory EquipInventory => equipInventory;

    [SerializeField] InventorySlotUI weaponSlot;
    //[SerializeField] InventorySlotUI helmetSlot;
    //[SerializeField] InventorySlotUI chestArmorSlot;

    void OnEnable()
    {
        equipInventory.OnEquipped += Refresh;
        equipInventory.OnUnequipped += Refresh;
    }

    //item 안씀
    public void Refresh(StoredItem item)
    {
        weaponSlot.Bind(equipInventory.Weapon);
    }
}

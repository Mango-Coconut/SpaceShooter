using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlotPanel : SlotPanelBase, ISlotPanel
{
    [SerializeField] EquipInventory equipInventory;
    public EquipInventory EquipInventory => equipInventory;
    protected override IItemSource GetSource() => equipInventory;


    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    [SerializeField] InventorySlotUI weaponSlot => uiSlots[(int)EquipIndex.Weapon];
    //[SerializeField] InventorySlotUI helmetSlot => uiSlots[(int)EquipIndex.Helmet];
    //[SerializeField] InventorySlotUI chestArmorSlot => uiSlots[(int)EquipIndex.ChestArmor];

    void OnEnable()
    {
        SubscribeInventory();
        SubscribeSlotUI();
    }
    void OnDisable()
    {
        UnSubscribeInventory();
        UnSubscribeSlotUI();
    }

    public void Refresh()
    {
        weaponSlot.Bind(equipInventory.Weapon);
    }

    
    void SubscribeInventory()
    {
        UnSubscribeInventory();
        equipInventory.OnChanged += Refresh;
    }
    void UnSubscribeInventory()
    {
        equipInventory.OnChanged -= Refresh;
    }
}

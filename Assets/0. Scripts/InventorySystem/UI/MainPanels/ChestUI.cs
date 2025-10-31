using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : InventoryUI
{
    Chest chestInventory;
    public Chest ChestInventory => chestInventory;

    public void SetChest(Chest chest)
    {
        chestInventory = chest;
        SetSlotPanel(chestInventory);
    }
    public void ClearChest()
    {
        chestInventory = null;
    }
}

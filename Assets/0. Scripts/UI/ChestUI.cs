using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : InventoryUI
{
    Chest chestInventory;
    public Chest ChestInventory => chestInventory;

    public void deliverChest(Chest chest)
    {
        chestInventory = chest;
        SetSlotPanel(chestInventory);
    }
}

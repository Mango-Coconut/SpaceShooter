using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPanel : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    public InventoryUI InventoryUI => inventoryUI;
    Chest chestInventory;
    public Chest ChestInventory => chestInventory;

    void Awake()
    {
        if (inventoryUI == null) inventoryUI = gameObject.GetComponent<InventoryUI>();
    }
    public void deliverChest(Chest chest)
    {
        chestInventory = chest;
        inventoryUI.SetSlotPanel(chestInventory);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPanel : MonoBehaviour
{
    [SerializeField] InventoryUI chestInventoryUI;
    public InventoryUI ChestInventoryUI => chestInventoryUI;
    Chest chestInventory;
    public Chest ChestInventory => chestInventory;

    void Awake()
    {
        if (chestInventoryUI == null) chestInventoryUI = gameObject.GetComponent<InventoryUI>();
    }
    public void deliverChest(Chest chest)
    {
        chestInventory = chest;
        chestInventoryUI.SetSlotPanel(chestInventory);
    }
}

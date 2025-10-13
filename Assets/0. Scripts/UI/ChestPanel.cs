using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPanel : MonoBehaviour
{
    [SerializeField] InventoryUI chestInventoryUI;
    public InventoryUI ChestInventoryUI => chestInventoryUI;
    Chest chestContainer;

    void Awake()
    {
        if (chestInventoryUI == null) chestInventoryUI = gameObject.GetComponent<InventoryUI>();
    }
    public void deliverChest(Chest chest)
    {
        chestContainer = chest;
        chestInventoryUI.GetSlotPanel().SetContainer(chestContainer);
    }
}

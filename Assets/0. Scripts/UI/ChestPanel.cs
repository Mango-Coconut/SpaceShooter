using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPanel : MonoBehaviour
{
    //각 슬롯 프리팹
    [SerializeField] SlotPanel slotPanel;
    [SerializeField] InventoryUI inventoryUI;
    Chest curChest;

    public void deliverChest(Chest chest)
    {
        curChest = chest;
        slotPanel.SetContainer(chest);
    }
}

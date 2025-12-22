using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] ShopSlotPanel shopSlotPanel;

    SlotEventBridge slotEventBridge = new SlotEventBridge();
    public SlotEventBridge SlotEventBridge => slotEventBridge;

    void OnEnable()
    {
        slotEventBridge.Subscribe(shopSlotPanel.Forwarder);
    }
    void OnDisable() 
    {
        slotEventBridge.UnSubscribe(shopSlotPanel.Forwarder);
    }

    public void Bind(ShopInventory inventory, int playerCoin)
    {
        shopSlotPanel.SetInventory(inventory);
        shopSlotPanel.SetCoin(playerCoin);
        shopSlotPanel.Refresh();
    }
}

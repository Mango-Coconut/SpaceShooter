using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] ShopSlotPanel shopSlotPanel;
    SlotPanelEventForwarder forwarder;

    public event Action<SlotPanelEventArgs> MouseEntered;
    public event Action<SlotPanelEventArgs> MouseExited;


    void Awake()
    {
        forwarder = shopSlotPanel.GetComponent<SlotPanelEventForwarder>();
    }

    public void SetSlotPanel(NpcMono npc)
    {
        shopSlotPanel.SetInventory(npc.ShopInventory);
        SubscribeShopSlotPanel();
    }

    void OnDisable() 
    {
        UnSubscribeShopSlotPanel();
    }

    void SubscribeShopSlotPanel()
    {
        UnSubscribeShopSlotPanel();
        forwarder.MouseEntered += ForwardMouseEnter;
        forwarder.MouseExited += ForwardMouseExit;
    }

    void UnSubscribeShopSlotPanel()
    {
        forwarder.MouseEntered -= ForwardMouseEnter;
        forwarder.MouseExited -= ForwardMouseExit;
    }

    void ForwardMouseEnter(SlotPanelEventArgs e)
    {
        MouseEntered?.Invoke(e);
    }

    void ForwardMouseExit(SlotPanelEventArgs e)
    {
        MouseExited?.Invoke(e);
    }

    public void Bind(ShopInventory inventory, int playerCoin)
    {
        shopSlotPanel.SetCoin(playerCoin);
        shopSlotPanel.SetInventory(inventory);
    }

    public void Clear()
    {

    }
}

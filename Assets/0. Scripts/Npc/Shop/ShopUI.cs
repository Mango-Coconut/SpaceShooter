using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] ShopSlotPanel shopSlotPanel;

    public event Action<SlotPanelEventArgs> MouseEntered;
    public event Action<SlotPanelEventArgs> MouseExited;

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
        shopSlotPanel.MouseEntered += ForwardMouseEnter;
        shopSlotPanel.MouseExited += ForwardMouseExit;
    }

    void UnSubscribeShopSlotPanel()
    {
        shopSlotPanel.MouseEntered -= ForwardMouseEnter;
        shopSlotPanel.MouseExited -= ForwardMouseExit;
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

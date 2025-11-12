using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    //인벤토리 슬롯
    [SerializeField] SlotPanel slotPanel;
    public SlotPanel SlotPanel => slotPanel;

    //장비 슬롯
    [SerializeField] EquipSlotPanel equipSlotPanel;
    public EquipSlotPanel EquipSlotPanel => equipSlotPanel;

    [SerializeField] CoinPanel coinPanel;

    //구독 편하게 하기 용
    SlotPanelBase[] panels;

    void Awake()
    {
        panels = GetComponentsInChildren<SlotPanelBase>(true);
    }

    void OnEnable()
    {
        SubscribeSlotPanel();
    }

    private void OnDisable()
    {
        UnsubscribeSlotPanel();
    }

    public void SetSlotPanel(Chest chest)
    {
        slotPanel.SetInventory(chest);
        SubscribeSlotPanel();
    }

    private void SubscribeSlotPanel()
    {
        UnsubscribeSlotPanel();

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].MouseEntered += ForwardMouseEnter;
            panels[i].MouseExited += ForwardMouseExit;
            panels[i].RightClicked += ForwardRightClick;
            panels[i].DragBegan += ForwardBeginDrag;
            panels[i].Dragging += ForwardDragging;
            panels[i].DragEnded += ForwardDropped;
        }
        
        slotPanel.OnChangedCoin += RefreshCoinPanel;
    }

    private void UnsubscribeSlotPanel()
    {
        if (panels == null)
        {
            NullChecker.NullCheck(this, nameof(panels));
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].MouseEntered -= ForwardMouseEnter;
            panels[i].MouseExited -= ForwardMouseExit;
            panels[i].RightClicked -= ForwardRightClick;
            panels[i].DragBegan -= ForwardBeginDrag;
            panels[i].Dragging -= ForwardDragging;
            panels[i].DragEnded -= ForwardDropped;
        }
        slotPanel.OnChangedCoin -= RefreshCoinPanel;
    }


    void RefreshCoinPanel(int coin)
    {
        coinPanel.SetCoin(coin);
    }

    // 슬롯 이벤트 포워딩
    public event Action<SlotPanelEventArgs> OnMouseEnter;
    public event Action<SlotPanelEventArgs> OnMouseExit;
    public event Action<SlotPanelEventArgs> OnRightClick;
    public event Action<SlotPanelEventArgs> OnBeginDrag;
    public event Action<SlotPanelEventArgs> OnDragging;
    public event Action<SlotPanelEventArgs> OnDropped;

    void ForwardMouseEnter(SlotPanelEventArgs args) => OnMouseEnter?.Invoke(args);
    void ForwardMouseExit(SlotPanelEventArgs args) => OnMouseExit?.Invoke(args);
    void ForwardRightClick(SlotPanelEventArgs args) => OnRightClick?.Invoke(args);
    void ForwardBeginDrag(SlotPanelEventArgs args) => OnBeginDrag?.Invoke(args);
    void ForwardDragging(SlotPanelEventArgs args) => OnDragging?.Invoke(args);
    void ForwardDropped(SlotPanelEventArgs args) => OnDropped?.Invoke(args);
}


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
    }

    // 슬롯 이벤트 포워딩
    public event Action<SlotPanelEventArgs> MouseEntered;
    public event Action<SlotPanelEventArgs> MouseExited;
    public event Action<SlotPanelEventArgs> RightClicked;
    public event Action<SlotPanelEventArgs> DragBegan;
    public event Action<SlotPanelEventArgs> Dragging;
    public event Action<SlotPanelEventArgs> DragEnded;

    void ForwardMouseEnter(SlotPanelEventArgs args) => MouseEntered?.Invoke(args);
    void ForwardMouseExit(SlotPanelEventArgs args) => MouseExited?.Invoke(args);
    void ForwardRightClick(SlotPanelEventArgs args) => RightClicked?.Invoke(args);
    void ForwardBeginDrag(SlotPanelEventArgs args) => DragBegan?.Invoke(args);
    void ForwardDragging(SlotPanelEventArgs args) => Dragging?.Invoke(args);
    void ForwardDropped(SlotPanelEventArgs args) => DragEnded?.Invoke(args);
}


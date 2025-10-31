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
        if (panels == null)
        {
            NullChecker.NullCheck(this, nameof(panels));
            return;
        }

        UnsubscribeSlotPanel();

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].OnMouseEnter += ForwardMouseEnter;
            panels[i].OnMouseExit += ForwardMouseExit;
            panels[i].OnRightClickArgs += ForwardRightClick;
            panels[i].OnBeginDragArgs += ForwardBeginDrag;
            panels[i].OnDraggingArgs += ForwardDragging;
            panels[i].OnDroppedArgs += ForwardDropped;
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
            panels[i].OnMouseEnter -= ForwardMouseEnter;
            panels[i].OnMouseExit -= ForwardMouseExit;
            panels[i].OnRightClickArgs -= ForwardRightClick;
            panels[i].OnBeginDragArgs -= ForwardBeginDrag;
            panels[i].OnDraggingArgs -= ForwardDragging;
            panels[i].OnDroppedArgs -= ForwardDropped;
        }
    }


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


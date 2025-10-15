using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private SlotPanel slotPanel;
    public SlotPanel SlotPanel => slotPanel;

    // ── 위로 포워딩할 이벤트 (PanelManager 등 상위에서 구독) ──
    public event Action<InventorySlotUI> ShowTooltip;
    public event Action HideTooltip;
    public event Action<InventorySlotUI, SlotPanel, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, SlotPanel, PointerEventData> Dropped;

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
        if (slotPanel == null)
        {
            NullChecker.NullCheck(this, nameof(slotPanel));
            return;
        }

        UnsubscribeSlotPanel();
        slotPanel.TooltipShown += OnSlotPanelTooltipShown;
        slotPanel.TooltipHidden += OnSlotPanelTooltipHidden;
        slotPanel.BeginDrag += OnSlotPanelBeginDrag;
        slotPanel.Dragging += OnSlotPanelDragging;
        slotPanel.Dropped += OnSlotPanelDropped;
    }

    private void UnsubscribeSlotPanel()
    {
        if (slotPanel == null)
        {
            return;
        }

        slotPanel.TooltipShown -= OnSlotPanelTooltipShown;
        slotPanel.TooltipHidden -= OnSlotPanelTooltipHidden;
        slotPanel.BeginDrag -= OnSlotPanelBeginDrag;
        slotPanel.Dragging -= OnSlotPanelDragging;
        slotPanel.Dropped -= OnSlotPanelDropped;
    }

    // ── SlotPanel → InventoryUI 포워딩 핸들러 ──
    private void OnSlotPanelTooltipShown(InventorySlotUI slotUI)
    {
        ShowTooltip?.Invoke(slotUI);
    }

    private void OnSlotPanelTooltipHidden(InventorySlotUI slotUI)
    {
        HideTooltip?.Invoke();
    }

    private void OnSlotPanelBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        BeginDrag?.Invoke(slotUI, slotPanel, e);
    }

    private void OnSlotPanelDragging(InventorySlotUI slotUI, PointerEventData e)
    {
        Dragging?.Invoke(slotUI, e);
    }

    private void OnSlotPanelDropped(InventorySlotUI slotUI, PointerEventData e)
    {
        Dropped?.Invoke(slotUI, slotPanel, e);
    }
}
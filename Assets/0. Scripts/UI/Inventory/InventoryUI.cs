using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private SlotPanel slotPanel;

    // ── 위로 포워딩할 이벤트 (PanelManager 등 상위에서 구독) ──
    public event Action<InventorySlotUI> ShowTooltip;
    public event Action HideTooltip;
    public event Action<InventorySlotUI, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

    private void OnEnable()
    {
        SubscribeSlotPanel();
    }
    void Start()
    {
        SubscribeSlotPanel();
    }

    private void OnDisable()
    {
        UnsubscribeSlotPanel();
    }

    public SlotPanel GetSlotPanel()
    {
        return slotPanel;
    }

    private void SubscribeSlotPanel()
    {
        if (slotPanel == null)
        {
            NullChecker.NullCheck(this, nameof(slotPanel));
            return;
        }


        // 중복 구독 방지용으로 일단 제거 후 등록
        slotPanel.TooltipShown -= OnSlotPanelTooltipShown;
        slotPanel.TooltipHidden -= OnSlotPanelTooltipHidden;
        slotPanel.BeginDrag -= OnSlotPanelBeginDrag;
        slotPanel.Dragging -= OnSlotPanelDragging;
        slotPanel.Dropped -= OnSlotPanelDropped;

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
            NullChecker.NullCheck(this, nameof(slotPanel));
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
        Action<InventorySlotUI> handler = ShowTooltip;
        if (handler != null) handler.Invoke(slotUI);
    }

    private void OnSlotPanelTooltipHidden(InventorySlotUI slotUI)
    {
        Action handler = HideTooltip;
        if (handler != null) handler.Invoke();
    }

    private void OnSlotPanelBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Action<InventorySlotUI, PointerEventData> handler = BeginDrag;
        if (handler != null) handler.Invoke(slotUI, e);
    }

    private void OnSlotPanelDragging(InventorySlotUI slotUI, PointerEventData e)
    {
        Action<InventorySlotUI, PointerEventData> handler = Dragging;
        if (handler != null) handler.Invoke(slotUI, e);
    }

    private void OnSlotPanelDropped(InventorySlotUI slotUI, PointerEventData e)
    {
        Action<InventorySlotUI, PointerEventData> handler = Dropped;
        if (handler != null) handler.Invoke(slotUI, e);
    }
}
using System;
using UnityEngine;

public class InventoryUIController
{
    InventoryUI inventoryUI;
    TooltipUIController tooltip;
    DragUIController dragdrop;

    // 외부(PanelManager 등)에서 정책적으로 쓰고 싶을 때 구독
    public event Action<InventorySlotUI, StoredItem> OnRightClickSlot;

    // 필요시: 인벤토리 열릴 때/닫힐 때 알림
    public event Action OnOpened;
    public event Action OnClosed;

    public bool IsOpen
    {
        get { return inventoryUI != null && inventoryUI.gameObject.activeSelf; }
    }

    public InventoryUIController(InventoryUI inventoryUI, TooltipUIController tooltip, DragUIController dragdrop)
    {
        this.inventoryUI = inventoryUI;
        this.tooltip = tooltip;
        this.dragdrop = dragdrop;
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();

        // 비활성화 시 후처리(서비스가 있으면 요청만)
        if (tooltip != null) tooltip.Hide();
        if (dragdrop != null) dragdrop.CancelIfDragging();
    }
    void Subscribe()
    {
        if (inventoryUI == null) return;
        Unsubscribe(); // 중복 방지

        inventoryUI.OnMouseEnter += ;
        inventoryUI.OnMouseExit += ;
        inventoryUI.OnBeginDrag += ;
        inventoryUI.OnDragging += ;
        inventoryUI.OnDropped += ;
    }

    void Unsubscribe()
    {
        if (inventoryUI == null) return;

    }
    /// <summary>
    /// 인벤토리 UI 열기(컨트롤러 관점에서 필요한 부수효과 포함)
    /// </summary>
    public void Open()
    {
        if (ui == null) return;
        if (!IsOpen)
        {
            ui.Open(); // UI(View) 내부에서 gameObject.SetActive(true) 등 수행
            CursorController.Apply(false);
            if (OnOpened != null) OnOpened();
        }
    }

    /// <summary>
    /// 인벤토리 UI 닫기(툴팁/드래그 등 부수효과 함께 정리)
    /// </summary>
    public void Close()
    {
        if (ui == null) return;
        if (IsOpen)
        {
            // 드래그 중이면 취소
            if (dragdrop != null) dragdrop.CancelIfDragging();

            // 툴팁 숨기기
            if (tooltip != null) tooltip.Hide();

            ui.Close(); // UI(View) 내부에서 gameObject.SetActive(false) 등 수행
            CursorController.Apply(true);
            if (OnClosed != null) OnClosed();
        }
    }

    /// <summary>
    /// 열려 있으면 닫고, 아니면 연다.
    /// </summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    // ========= 슬롯 포인터 → 툴팁 =========

    // UI(View)가 제공하는 시그니처 가정:
    // OnPointerEnterSlot(InventorySlotUI slot, StoredItem item)
    void HandlePointerEnterSlot(InventorySlotUI slot, StoredItem item)
    {
        if (tooltip == null) return;
        if (slot == null) { tooltip.Hide(); return; }

        // 슬롯의 RectTransform을 기준 앵커로 사용
        tooltip.Show(item, slot.Rect);
    }

    // OnPointerExitSlot()
    void HandlePointerExitSlot()
    {
        if (tooltip == null) return;
        tooltip.Hide();
    }

    // OnPointerMove(Vector2 screenPos)
    void HandlePointerMove(Vector2 screenPos)
    {
        if (tooltip == null) return;
        tooltip.MoveTo(screenPos);
    }

    // ========= 슬롯 우클릭 → 외부 정책으로 전달 =========

    // OnRightClickSlot(InventorySlotUI slot, StoredItem item)
    void HandleRightClickSlot(InventorySlotUI slot, StoredItem item)
    {
        Action<InventorySlotUI, StoredItem> handler = OnRightClickSlot;
        if (handler != null) handler(slot, item);
    }

    // ========= 드래그 → DragDropController 위임 =========

    // OnBeginDrag(StoredItem item, PointerEventData e)  (또는 해당 프로젝트 시그니처에 맞게)
    void HandleBeginDrag(StoredItem item, UnityEngine.EventSystems.PointerEventData e)
    {
        if (dragdrop == null) return;
        dragdrop.HandleBegin(item, e);
    }

    // OnDragging(PointerEventData e)
    void HandleDragging(UnityEngine.EventSystems.PointerEventData e)
    {
        if (dragdrop == null) return;
        dragdrop.HandleDrag(e);
    }

    // OnEndDrag(StoredItem item, PointerEventData e)
    void HandleEndDrag(StoredItem item, UnityEngine.EventSystems.PointerEventData e)
    {
        if (dragdrop == null) return;
        dragdrop.HandleEnd(item, e);
    }
}
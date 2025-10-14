using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Interactor interactor;

    [Header("InventoryPanels")]
    [SerializeField] InventoryUI inventoryUI;

    [Header("InteractionPanels")]
    [SerializeField] InteractionPanel iiPanel;

    [Header("ChestPanel")]
    [SerializeField] ChestPanel chestPanel; // ← Chest 쪽도 동일 시그니처의 이벤트를 포워딩한다고 가정

    [Header("TooltipUI")]
    [SerializeField] TooltipUI tooltipUI;

    [Header("DragSlot")]
    [SerializeField] DragSlot dragSlot;

    [Header("UI Canvas / Camera (for positioning)")]
    RectTransform uiRect;
    Camera cam = null;

    bool isInvenOpen;
    bool isChestOpen;

    void OnEnable()
    {
        if (interactor != null) interactor.TargetChanged += IiPanelChange;
        Chest.OnChestOpened += ChestUIToggle;

        if (InputManager.Instance != null)
        {        
            InputManager.Instance.OnToggleInventory += InventoryUIToggle;
            InputManager.Instance.OnEsc += InventoryUIHandleEsc;    
        }
    }

    void OnDisable()
    {
        if (interactor != null) interactor.TargetChanged -= IiPanelChange;
        Chest.OnChestOpened -= ChestUIToggle;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
            InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
        }
    }
    
    void Awake()
    {
        if (uiRect == null) uiRect = GetComponent<RectTransform>();
        //Canvas Mode Overlay일 경우 null
        //if (cam == null) cam = Camera.main;
    }
    void Start()
    {
        inventoryUI.gameObject.SetActive(false);
        iiPanel.gameObject.SetActive(false);
        chestPanel.gameObject.SetActive(false);

        tooltipUI.gameObject.SetActive(false);
        dragSlot.gameObject.SetActive(false);
    }

    // ───────────────────────────────── UI 토글/연계 ─────────────────────────────────

    void IiPanelChange(IInteractable interactable)
    {
        bool show = interactable != null;
        iiPanel.gameObject.SetActive(show);
        if (show) iiPanel.OnTargetChange(interactable);
    }

    void InventoryUIHandleEsc()
    {
        if (isInvenOpen || isChestOpen)
        {
            CloseInventoryUI();
            CloseChestUI();
        }
        else
        {
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }

    void InventoryUIToggle()
    {
        if (!isInvenOpen) OpenInventoryUI();
        else
        {
            CloseInventoryUI();
            CloseChestUI();
        }
    }

    void OpenInventoryUI()
    {
        isInvenOpen = true;
        inventoryUI.gameObject.SetActive(true);
        SubscribeInventoryUI();
        CursorController.Apply(false);
    }

    void CloseInventoryUI()
    {
        isInvenOpen = false;
        inventoryUI.gameObject.SetActive(false);
        CloseTooltip();
        CursorController.Apply(true);
    }

    void ChestUIToggle(Chest c)
    {
        if (!isChestOpen)
        {
            OpenInventoryUI();
            OpenChestUI(c);
        }
        else
        {
            CloseInventoryUI();
            CloseChestUI();
        }
    }

    void OpenChestUI(Chest c)
    {
        isChestOpen = true;
        chestPanel.gameObject.SetActive(true);
        chestPanel.deliverChest(c);
        SubscribeChestUI();
    }

    void CloseChestUI()
    {
        isChestOpen = false;
        chestPanel.gameObject.SetActive(false);
    }

    // ───────────────────────────── Tooltip/Drag 구현 (핵심) ─────────────────────────────

    // InventoryUI / ChestPanel 이벤트 구독
    void SubscribeInventoryUI()
    {
        if (inventoryUI == null)
        {
            NullChecker.NullCheck(this, nameof(inventoryUI));
            return;
        }

        // 중복 방지
        UnsubscribeInventoryUI();

        inventoryUI.ShowTooltip += OpenTooltip;
        inventoryUI.HideTooltip += CloseTooltip;
        inventoryUI.BeginDrag += OnBeginDragFromPanel;
        inventoryUI.Dragging += OnDraggingFromPanel;
        inventoryUI.Dropped += OnDroppedFromPanel;
    }

    void UnsubscribeInventoryUI()
    {
        if (inventoryUI == null)
        {
            NullChecker.NullCheck(this, nameof(inventoryUI));
            return;
        }

        inventoryUI.ShowTooltip -= OpenTooltip;
        inventoryUI.HideTooltip -= CloseTooltip;
        inventoryUI.BeginDrag -= OnBeginDragFromPanel;
        inventoryUI.Dragging -= OnDraggingFromPanel;
        inventoryUI.Dropped -= OnDroppedFromPanel;
    }

    void SubscribeChestUI()
    {
        if (chestPanel == null || chestPanel.ChestInventoryUI == null)
        {
            NullChecker.NullCheck(this, nameof(chestPanel));
            return;
        }
        UnsubscribeChestUI();
        var ui = chestPanel.ChestInventoryUI;
        ui.ShowTooltip += OpenTooltip;
        ui.HideTooltip += CloseTooltip;
        ui.BeginDrag += OnBeginDragFromPanel;
        ui.Dragging += OnDraggingFromPanel;
        ui.Dropped += OnDroppedFromPanel;
    }

    void UnsubscribeChestUI()
    {
        if (chestPanel == null || chestPanel.ChestInventoryUI == null)
        {
            NullChecker.NullCheck(this, nameof(chestPanel));
            return;
        }

        var ui = chestPanel.ChestInventoryUI;
        ui.ShowTooltip -= OpenTooltip;
        ui.HideTooltip -= CloseTooltip;
        ui.BeginDrag -= OnBeginDragFromPanel;
        ui.Dragging -= OnDraggingFromPanel;
        ui.Dropped -= OnDroppedFromPanel;
    }

    // ▼ Tooltip
    void OpenTooltip(InventorySlotUI slotUI)
    {
        if (slotUI == null || slotUI.EnterItem == null)
        {
            return;
        }
        // 슬롯 RectTransform
        RectTransform slotRect = slotUI.Rect;

        // 슬롯의 월드 좌표 → 우상단 꼭짓점
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2]; // ↗

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldTopRight),
            cam,
            out localPos
        );
        // 툴팁 위치 및 표시
        RectTransform ttRect = (RectTransform)tooltipUI.transform;
        ttRect.anchoredPosition = localPos;

        tooltipUI.Set(slotUI.EnterItem);
        tooltipUI.gameObject.SetActive(true);
    }

    void CloseTooltip()
    {
        tooltipUI.gameObject.SetActive(false);
    }

    // ▼ Drag
    void OnBeginDragFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 시작 시 툴팁 강제 숨김
        CloseTooltip();

        // TODO: slotUI에서 아이템 참조를 얻어와 DragSlot에 바인딩
        // var item = slotUI.GetBoundItem();
        // if (item == null) return;

        // dragSlot.Bind(item);
        // dragSlot.gameObject.SetActive(true);
        // MoveDragVisual(e.position);

        // 애매한 부분: 프로젝트 슬롯 API에 맞춰 채우기
    }

    void OnDraggingFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 아이콘 따라다니기
        MoveDragVisual(e.position);
    }

    void OnDroppedFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        // 1) 위치로 드롭 대상 판정 (인벤/체스트)
        //    RectTransformUtility.RectangleContainsScreenPoint(…)
        // 2) 컨테이너 간 아이템 이동 로직 호출 (원자적 이동)
        // 3) 드래그 비주얼 숨김

        // TODO: 드롭 정책/판정은 기존 프로젝트 방식대로 구현


        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        raycaster.Raycast(e, results);
        string tag = "";

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                tag = "InventoryUI";
                return;
            }
            if (result.gameObject.CompareTag("ChestUI"))
            {
                tag = "ChestUI";
                return;
            }
        }
        dragSlot.gameObject.SetActive(false);
    }

    void MoveDragVisual(Vector2 screenPos)
    {
        if (!dragSlot.gameObject.activeSelf) return;

        RectTransform dragRect = (RectTransform)dragSlot.transform;
        Vector2 localPos;

        if (uiRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiRect,
                screenPos,
                cam,
                out localPos
            );
            dragRect.anchoredPosition = localPos;
        }
        else
        {
            // fallback
            dragRect.position = screenPos;
        }
    }

    // 기존 ShowTooltip(StoredItem, RectTransform)
    public void ShowTooltip(StoredItem item, RectTransform slotRect)
    {
        // 위치 계산 (슬롯 우측 상단 기준)
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        RectTransform ttRect = (RectTransform)tooltipUI.transform;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldTopRight),
            cam,
            out localPos
        );
        ttRect.anchoredPosition = localPos;

        tooltipUI.Set(item);
        tooltipUI.gameObject.SetActive(true);
    }
}
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
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
    [SerializeField] ChestPanel chestPanel;

    [Header("TooltipUI")]
    [SerializeField] TooltipUI tooltipUI;

    [Header("DragSlot")]
    [SerializeField] DragSlot dragSlot;

    [Header("UI Canvas")]
    RectTransform uiRect;

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

        SubscribeInventoryUI();
        SubscribeChestUI();
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

        UnsubscribeInventoryUI();
        UnsubscribeChestUI();
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

    //static 이벤트는 destroy에서도 구독 해제
    void OnDestroy()
    {
        Chest.OnChestOpened -= ChestUIToggle;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
            InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
        }
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
        Debug.Log("chestclosed");
        //상자 닫힐 때 플레이어 행동 가능하게
        PlayerActionGate.Instance.PopInteract();
        chestPanel.gameObject.SetActive(false);
    }

    // InventoryUI / ChestPanel 이벤트 구독
    void SubscribeInventoryUI()
    {
        if (inventoryUI == null) return;

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
        if (inventoryUI == null) return;

        inventoryUI.ShowTooltip -= OpenTooltip;
        inventoryUI.HideTooltip -= CloseTooltip;
        inventoryUI.BeginDrag -= OnBeginDragFromPanel;
        inventoryUI.Dragging -= OnDraggingFromPanel;
        inventoryUI.Dropped -= OnDroppedFromPanel;
    }

    void SubscribeChestUI()
    {
        if (chestPanel == null || chestPanel.ChestInventoryUI == null) return;

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
        if (chestPanel == null || chestPanel.ChestInventoryUI == null) return;

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
        if (slotUI == null || slotUI.EnterItem == null) return;
        // 슬롯 RectTransform
        RectTransform slotRect = slotUI.Rect;

        // 슬롯의 월드 좌표 → 우상단 꼭짓점
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, worldTopRight);

        // 툴팁 위치 및 표시
        RectTransform ttRect = (RectTransform)tooltipUI.transform;
        ttRect.position = screen;

        tooltipUI.Set(slotUI.EnterItem);
        tooltipUI.gameObject.SetActive(true);
    }

    void CloseTooltip()
    {
        tooltipUI.gameObject.SetActive(false);
    }

    SlotPanel fromSP;
    void OnBeginDragFromPanel(InventorySlotUI slotUI, SlotPanel sp, PointerEventData e)
    {
        if (slotUI.EnterItem == null) return;

        //드래그 시작한 곳(인벤토리or상자)
        fromSP = sp;
        CloseTooltip();

        dragSlot.gameObject.SetActive(true);
        dragSlot.Bind(slotUI.EnterItem);
        StoredItem i = slotUI.EnterItem;
    }

    void OnDraggingFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        dragSlot.transform.position = e.position;
    }

    void OnDroppedFromPanel(InventorySlotUI slotUI, SlotPanel sp, PointerEventData e)
    {
        SlotPanel toSP = sp;
        ItemData data = slotUI.EnterItem.itemdata;
        int amount = slotUI.EnterItem.count;
        if (fromSP == null) return;
        if (data == null) return;

        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        raycaster.Raycast(e, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                toSP = inventoryUI.SlotPanel;
            }
            if (result.gameObject.CompareTag("ChestUI"))
            {
                toSP = chestPanel.ChestInventoryUI.SlotPanel;
            }
        }
        
        //시작과 끝이 서로 다르면 추가 제거
        if (!ReferenceEquals(fromSP, toSP))
        {
            InventoryManager.Instance.TryRemoveItem(fromSP.Container, data, amount);
            InventoryManager.Instance.TryAddItem(toSP.Container, data, amount);
        }        
        dragSlot.gameObject.SetActive(false);
    }
}
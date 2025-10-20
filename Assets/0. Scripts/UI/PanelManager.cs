using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    #region 보유하고 있는 하위 패널들 연결
    [SerializeField] Interactor interactor;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ChestUI chestUI; //public class ChestUI : InventoryUI { ... }
    [SerializeField] EquipSlotPanel equipSlotPanel;
    [SerializeField] TooltipUI tooltipUI;
    [SerializeField] DragSlot dragSlot;
    [SerializeField] InteractionPanel iiPanel;
    #endregion

    void OnEnable()
    {
        //Inventory 이벤트
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory += InventoryUIToggle;
            InputManager.Instance.OnEsc += InventoryUIHandleEsc;
        }
        //Chest 이벤트()
        Chest.OnChestOpened += ChestUIToggle;

        //InventoryUI 이벤트
        SubscribeInventoryUI(inventoryUI);
        //ChestUI 이벤트
        SubscribeInventoryUI(chestUI);
        //EquipSlotPanel 이벤트(unequipped 장비 반환)
        equipSlotPanel.UnequippedItemReturn += TryAddItem;

        //InteractPanel 이벤트
        if (interactor != null) interactor.TargetChanged += IiPanelChange;
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
            InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
        }
        Chest.OnChestOpened -= ChestUIToggle;

        UnsubscribeInventoryUI(inventoryUI);
        UnsubscribeInventoryUI(chestUI);
        equipSlotPanel.UnequippedItemReturn -= TryAddItem;

        if (interactor != null) interactor.TargetChanged -= IiPanelChange;
    }

    void Awake()
    {
        inventoryUI.gameObject.SetActive(true);
        iiPanel.gameObject.SetActive(true);
        chestUI.gameObject.SetActive(true);
        tooltipUI.gameObject.SetActive(true);
        dragSlot.gameObject.SetActive(true);
    }
    void Start()
    {
        inventoryUI.gameObject.SetActive(false);
        iiPanel.gameObject.SetActive(false);
        chestUI.gameObject.SetActive(false);
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


    #region UI 열고 닫기 (Invnetory, Chest)

    bool isInvenOpen;
    bool isChestOpen;
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
        SubscribeInventoryUI(inventoryUI);
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
        chestUI.gameObject.SetActive(true);
        chestUI.deliverChest(c);
        SubscribeInventoryUI(chestUI);
    }

    void CloseChestUI()
    {
        isChestOpen = false;
        //상자 닫힐 때 플레이어 행동 가능하게
        PlayerActionGate.Instance.PopInteract();
        chestUI.gameObject.SetActive(false);
    }
    #endregion

    #region 이벤트 구독 (InventoryUI, ChestUI)
    void SubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) return;

        // 중복 방지
        UnsubscribeInventoryUI(inventoryUI);

        inventoryUI.ShowTooltip += OpenTooltip;
        inventoryUI.HideTooltip += CloseTooltip;
        inventoryUI.BeginDrag += OnBeginDragFromPanel;
        inventoryUI.Dragging += OnDraggingFromPanel;
        inventoryUI.Dropped += OnDroppedFromPanel;
    }

    void UnsubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) return;

        inventoryUI.ShowTooltip -= OpenTooltip;
        inventoryUI.HideTooltip -= CloseTooltip;
        inventoryUI.BeginDrag -= OnBeginDragFromPanel;
        inventoryUI.Dragging -= OnDraggingFromPanel;
        inventoryUI.Dropped -= OnDroppedFromPanel;
    }
    #endregion

    #region 아이템 툴팁 열고 닫기
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
    #endregion

    #region InteractPanel 열고 닫기
    void IiPanelChange(IInteractable interactable)
    {
        bool show = interactable != null;
        iiPanel.gameObject.SetActive(show);
        if (show) iiPanel.OnTargetChange(interactable);
    }
    #endregion

    #region 아이템 슬롯 드래그 하기 & 아이템 옮기기(인벤토리↔상자)
    IStorable fromStorage = null;
    void OnBeginDragFromPanel(InventorySlotUI slotUI, IStorable storage, PointerEventData e)
    {
        fromStorage = null;
        toStorage = null;
        if (slotUI.EnterItem == null) return;

        //드래그 시작한 곳(인벤토리or상자)
        fromStorage = storage;
        CloseTooltip();

        dragSlot.gameObject.SetActive(true);
        dragSlot.Bind(slotUI.EnterItem);
        StoredItem i = slotUI.EnterItem;
    }

    void OnDraggingFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        dragSlot.transform.position = e.position;
    }

    IStorable toStorage = null;
    void OnDroppedFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        StoredItem item = slotUI.EnterItem;
        if (fromStorage == null) return;
        if (item == null || item.itemdata == null) return;

        // 마우스 놓은 Storage 창 구하기(인벤토리, 장비창, 상자)
        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        raycaster.Raycast(e, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                toStorage = inventoryUI.SlotPanel.Inventory;
            }
            if (result.gameObject.CompareTag("ChestUI"))
            {
                toStorage = chestUI.ChestInventory;
            }
            if (result.gameObject.CompareTag("EquipUI"))
            {
                toStorage = equipSlotPanel.EquipInventory;
            }
        }
        if (toStorage == null) return;

        //시작과 끝이 서로 다르면 추가 제거
        if (!ReferenceEquals(fromStorage, toStorage))
        {
            bool isAdd = InventoryManager.Instance.TryAddItem(toStorage, item);
            if (isAdd)
            {
                bool isRemove = InventoryManager.Instance.TryRemoveItem(fromStorage, item);
            }
        }
        dragSlot.gameObject.SetActive(false);
    }

    //equip에서 장착 제거한 아이템 반환시 fromStorage에 넣기
    void TryAddItem(StoredItem item)
    {
        if(fromStorage == null)
        {
            Debug.Log($"fromstorage null");
            return;
        }
        bool isAdd = InventoryManager.Instance.TryAddItem(fromStorage, item);
    }
    void TryRemoveItem(StoredItem item)
    {
        
    }
    #endregion
}
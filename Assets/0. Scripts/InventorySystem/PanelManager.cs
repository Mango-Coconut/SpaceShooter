using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    #region 보유하고 있는 하위 패널들 연결
    [SerializeField] Interactor interactor;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ChestUI chestUI; //public class ChestUI : InventoryUI { ... }
    Chest curChest;
    [SerializeField] EquipSlotPanel equipSlotPanel;
    [SerializeField] TooltipUI tooltipUI;
    [SerializeField] DragSlot dragSlot;
    [SerializeField] InteractionPanel iiPanel;
    #endregion

    public event Action<StorageTarget, StorageTarget, StoredItem> OnItemDropped;
    public event Action<StoredItem, StorageTarget> OnItemRightClicked;
    public event Action OnChestClosed;


    GraphicRaycaster raycaster;

    public bool IsOpen(GameObject gameObject)
    {
        return gameObject != null && gameObject.activeSelf;
    }

    void OnEnable()
    {
        //Inventory 이벤트
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory += InventoryUIToggle;
            InputManager.Instance.OnEsc += InventoryUIHandleEsc;
        }
        //Chest 이벤트()
        Chest.OnChestOpened += HandleChestOpened;
        Chest.OnChestClosed += HandleChestClosed;

        //InventoryUI 이벤트
        SubscribeInventoryUI(inventoryUI);
        //ChestUI 이벤트
        SubscribeInventoryUI(chestUI);

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
        Chest.OnChestOpened -= HandleChestOpened;
        Chest.OnChestClosed -= HandleChestClosed;

        UnsubscribeInventoryUI(inventoryUI);
        UnsubscribeInventoryUI(chestUI);

        if (interactor != null) interactor.TargetChanged -= IiPanelChange;
    }

    void Awake()
    {
        raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        //fromStorage = inventoryUI.SlotPanel.Inventory.Core;
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
        Chest.OnChestOpened -= HandleChestOpened;
        Chest.OnChestClosed -= HandleChestClosed;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
            InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
        }
    }


    #region Inventory UI 열고 닫기

    void InventoryUIHandleEsc()
    {
        if (IsOpen(inventoryUI.gameObject))
        {
            CloseInventoryUI();
            if(curChest) curChest.ForceCloseFromUI();
        }
        else
        {
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }

    void InventoryUIToggle()
    {
        if (!IsOpen(inventoryUI.gameObject))
        {
            OpenInventoryUI();
        }
        else
        {
            CloseInventoryUI();
            if(curChest) curChest.ForceCloseFromUI();
        }
    }

    void OpenInventoryUI()
    {
        inventoryUI.gameObject.SetActive(true);
        CursorController.Apply(false);
    }

    void CloseInventoryUI()
    {
        inventoryUI.gameObject.SetActive(false);
        CloseTooltip();
        CursorController.Apply(true);
    }

    #endregion

    #region Chest UI 열고 닫기

    void HandleChestOpened(Chest c)
    {
        curChest = c;

        OpenInventoryUI();

        chestUI.gameObject.SetActive(true);
        chestUI.SetChest(c);

        SubscribeInventoryUI(chestUI);

        iiPanel.gameObject.SetActive(false);
    }

    void HandleChestClosed(Chest c)
    {
        if (curChest == c) curChest = null;

        if (!IsOpen(chestUI.gameObject)) 
            return;

        chestUI.ClearChest();
        chestUI.gameObject.SetActive(false);

        UnsubscribeInventoryUI(chestUI);

        CloseInventoryUI();

        iiPanel.gameObject.SetActive(true);
    }
    #endregion

    #region 이벤트 구독 (InventoryUI, ChestUI)
    void SubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) return;

        // 중복 방지
        UnsubscribeInventoryUI(inventoryUI);

        inventoryUI.PointerEnter += OpenTooltip;
        inventoryUI.PointerExit += CloseTooltip;
        inventoryUI.RightClick += OnRightClick;
        inventoryUI.BeginDrag += OnBeginDragFromPanel;
        inventoryUI.Dragging += OnDraggingFromPanel;
        inventoryUI.EndDrag += OnDroppedFromPanel;
    }

    void UnsubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) return;

        inventoryUI.PointerEnter -= OpenTooltip;
        inventoryUI.PointerExit -= CloseTooltip;
        inventoryUI.RightClick -= OnRightClick;
        inventoryUI.BeginDrag -= OnBeginDragFromPanel;
        inventoryUI.Dragging -= OnDraggingFromPanel;
        inventoryUI.EndDrag -= OnDroppedFromPanel;
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
    StorageTarget fromStorage;
    StorageTarget toStorage;
    void OnBeginDragFromPanel(StoredItem item, StorageTarget from, PointerEventData e)
    {
        if (item == null) return;

        //드래그 시작한 곳(인벤토리or상자)
        fromStorage = from;
        CloseTooltip();

        dragSlot.gameObject.SetActive(true);
        dragSlot.Bind(item);
        StoredItem i = item;
    }

    void OnDraggingFromPanel(PointerEventData e)
    {
        dragSlot.transform.position = e.position;
    }


    void OnDroppedFromPanel(StoredItem item, PointerEventData e)
    {
        if (item == null || item.itemData == null) return;
        toStorage = StorageTarget.None;

        // 마우스 놓은 Storage 창 구하기(인벤토리, 장비창, 상자)
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(e, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                toStorage = StorageTarget.Player;
                break;
            }
            else if (result.gameObject.CompareTag("ChestUI"))
            {
                toStorage = StorageTarget.Chest;
                break;
            }
            else if (result.gameObject.CompareTag("EquipUI"))
            {
                toStorage = StorageTarget.Equip;
                break;
            }
        }

        if (toStorage == StorageTarget.None)
        {
            toStorage = StorageTarget.World;
        }

        OnItemDropped?.Invoke(fromStorage, toStorage, item);

        dragSlot.gameObject.SetActive(false);
    }
    public void OnRightClick(StoredItem item, StorageTarget from)
    {
        Log.Info($"PanelManager -> RightClick, {item.itemData.name}");
        CloseTooltip();
        OnItemRightClicked?.Invoke(item, from);
    }

    #endregion
}
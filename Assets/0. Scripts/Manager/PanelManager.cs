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
    [SerializeField] ChestUI chestUI;
    [SerializeField] NpcUI npcUI;
    Chest curChest;
    NpcMono curNpc;
    [SerializeField] EquipSlotPanel equipSlotPanel;
    [SerializeField] TooltipUI tooltipUI;
    [SerializeField] DragSlot dragSlot;
    [SerializeField] InteractionPanel iiPanel;
    #endregion

    public event Action<StorageTarget, StorageTarget, StoredItem> OnItemDropped;
    public event Action<StoredItem, StorageTarget> OnItemRightClicked;

    // Chest, Npc 등 전역 이벤트 연결
    [SerializeField] InteractionHub hub;


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
            InputManager.Instance.OnToggleInventory += HandleInventoryUIToggle;
            InputManager.Instance.OnEsc += HandleUIClose;
        }

        //InventoryUI 이벤트
        SubscribeInventoryUI(inventoryUI);
        //ChestUI 이벤트
        SubscribeInventoryUI(chestUI);
        // 전역 이벤트(Chest, Npc)
        SubscribeHubEvent();
        
        //InteractPanel 이벤트
        interactor.TargetChanged += HandleIiPanelChange;
    }
    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInventory -= HandleInventoryUIToggle;
            InputManager.Instance.OnEsc -= HandleUIClose;
        }

        UnsubscribeInventoryUI(inventoryUI);
        UnsubscribeInventoryUI(chestUI);
        UnSubscribeHubEvent();

        interactor.TargetChanged -= HandleIiPanelChange;
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


    #region Inventory UI 열고 닫기

    void HandleUIClose()
    {
        
    }

    void HandleInventoryUIToggle()
    {
        if (!IsOpen(inventoryUI.gameObject))
        {
            OpenInventoryUI();
        }
        else
        {
            CloseInventoryUI();
            curChest?.ForceCloseFromUI();
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

    void HandleChestOpen(Chest c)
    {
        if (curChest != null && curChest != c)
        {
            HandleChestClose(curChest);
        }
        curChest = c;
        chestUI.SetChest(c);

        PushUI(inventoryUI.gameObject, CloseInventoryUI, "Inventory");
        PushUI(chestUI.gameObject, CloseChestUI, "Chest");
        SubscribeInventoryUI(chestUI);

        iiPanel.gameObject.SetActive(false);
    }

    void HandleChestClose(Chest c)
    {
        if (curChest == c) curChest = null;

        if (!IsOpen(chestUI.gameObject))
            return;

        UnsubscribeInventoryUI(chestUI);
        chestUI.ClearChest();

        iiPanel.gameObject.SetActive(true);
    }
    void CloseChestUI()
    {
        HandleChestClose(curChest);
    }
    #endregion

    #region 이벤트 구독 (InventoryUI, ChestUI)
    void SubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) { return; }

        UnsubscribeInventoryUI(inventoryUI);

        inventoryUI.OnMouseEnter += HandlePointerEnter;
        inventoryUI.OnMouseExit += HandlePointerExit;
        inventoryUI.OnRightClick += HandleRightClick;
        inventoryUI.OnBeginDrag += HandleBeginDragFromPanel;
        inventoryUI.OnDragging += HandleDraggingFromPanel;
        inventoryUI.OnDropped += HandleEndDragFromPanel;
    }

    void UnsubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) { return; }

        inventoryUI.OnMouseEnter -= HandlePointerEnter;
        inventoryUI.OnMouseExit -= HandlePointerExit;
        inventoryUI.OnRightClick -= HandleRightClick;
        inventoryUI.OnBeginDrag -= HandleBeginDragFromPanel;
        inventoryUI.OnDragging -= HandleDraggingFromPanel;
        inventoryUI.OnDropped -= HandleEndDragFromPanel;
    }

    void SubscribeHubEvent()
    {
        UnSubscribeHubEvent();
        if (hub != null && hub.npc != null)
        {
            hub.npc.OnEnter += HandleNpcEnter;
            hub.npc.OnExit += HandleNpcExit;
        }
        if (hub != null && hub.chest != null)
        {
            hub.chest.OnOpen += HandleChestOpen;
            hub.chest.OnClose += HandleChestClose;
        }
    }

    void UnSubscribeHubEvent()
    {
        if (hub != null && hub.npc != null)
        {
            hub.npc.OnEnter -= HandleNpcEnter;
            hub.npc.OnExit  -= HandleNpcExit;
        }
        if (hub != null && hub.chest != null)
        {
            hub.chest.OnOpen -= HandleChestOpen;
            hub.chest.OnClose -= HandleChestClose;
        }
    }
    
    #endregion

    #region 아이템 툴팁 열고 닫기
    void HandlePointerEnter(SlotPanelEventArgs e)
    {
        InventorySlotUI slotUI = e.Slot;
        if (slotUI == null || slotUI.EnterItem == null) { return; }

        RectTransform slotRect = slotUI.Rect;

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, worldTopRight);

        RectTransform ttRect = (RectTransform)tooltipUI.transform;
        ttRect.position = screen;

        tooltipUI.Set(slotUI.EnterItem);
        tooltipUI.gameObject.SetActive(true);
    }


    void HandlePointerExit(SlotPanelEventArgs e)
    {
        CloseTooltip();
    }

    void CloseTooltip()
    {
        tooltipUI.gameObject.SetActive(false);
    }
    #endregion

    #region 아이템 슬롯 드래그 하기 & 아이템 옮기기(인벤토리↔상자)
    StorageTarget fromStorage;
    StorageTarget toStorage;
    void HandleBeginDragFromPanel(SlotPanelEventArgs e)
    {
        StoredItem item = e.Item;
        if (item == null) { return; }

        fromStorage = e.Source;  // 드래그 시작 지점(인벤/상자/장비)
        CloseTooltip();

        dragSlot.gameObject.SetActive(true);
        dragSlot.Bind(item);

        // e.Pointer 사용 가능 (필요하면)
    }

    // [~] OnDraggingFromPanel(PointerEventData) → HandleDraggingFromPanel(SlotPanelEventArgs)
    void HandleDraggingFromPanel(SlotPanelEventArgs e)
    {
        if (e.Pointer != null)
        {
            dragSlot.transform.position = e.Pointer.position;
        }
    }

    // [~] OnDroppedFromPanel(StoredItem, PointerEventData) → HandleEndDragFromPanel(SlotPanelEventArgs)
    void HandleEndDragFromPanel(SlotPanelEventArgs e)
    {
        StoredItem item = e.Item;
        if (item == null || item.itemData == null) { return; }

        toStorage = StorageTarget.None;

        // 마우스가 놓인 UI 판정
        if (e.Pointer != null)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(e.Pointer, results);

            for (int i = 0; i < results.Count; i++)
            {
                GameObject go = results[i].gameObject;
                if (go.CompareTag("InventoryUI"))
                {
                    toStorage = StorageTarget.Player;
                    break;
                }
                if (go.CompareTag("ChestUI"))
                {
                    toStorage = StorageTarget.Chest;
                    break;
                }
                if (go.CompareTag("EquipUI"))
                {
                    toStorage = StorageTarget.Equip;
                    break;
                }
            }
        }

        if (toStorage == StorageTarget.None)
        {
            toStorage = StorageTarget.World;
        }

        OnItemDropped?.Invoke(fromStorage, toStorage, item);

        dragSlot.gameObject.SetActive(false);
    }
    void HandleRightClick(SlotPanelEventArgs e)
    {
        StoredItem item = e.Item;
        StorageTarget from = e.Source;

        if (item == null || item.itemData == null) { return; }

        Log.Info($"PanelManager -> RightClick, {item.itemData.name}");
        CloseTooltip();

        // 외부 이벤트는 기존 시그니처 유지
        OnItemRightClicked?.Invoke(item, from);
    }

    #endregion

    #region InteractPanel 열고 닫기
    void HandleIiPanelChange(IInteractable interactable)
    {
        bool show = interactable != null;
        iiPanel.gameObject.SetActive(show);
        if (show) { iiPanel.OnTargetChange(interactable); }
    }
    #endregion

    #region NpcUI 열고 닫기
    void HandleNpcEnter(NpcMono npc)
    {
        if (curNpc != null && curNpc != npc)
        {
            HandleNpcExit(curNpc);
        }
        curNpc = npc;

        PushUI(npcUI.gameObject, CloseNpcFromTop, "NPC");
        npcUI.Bind(npc.Core);
        BindNpcCore(npc.Core, true);
    }

    void HandleNpcExit(NpcMono npc)
    {
        npcUI.gameObject.SetActive(false);
    }
    #endregion
}
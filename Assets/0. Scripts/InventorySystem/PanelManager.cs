using System;
using System.Collections.Generic;
using TMPro;
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

    #region 프로퍼티
    public Inventory PlayerInventory => inventoryUI.SlotPanel.Inventory;
    public Inventory ChestInventory => chestUI.ChestInventory;
    public EquipInventory EquipInventory => equipSlotPanel.EquipInventory;

    public InventoryUI InventoryUI => inventoryUI;
    public ChestUI ChestUI => chestUI;
    public EquipSlotPanel EquipUI => equipSlotPanel;
    #endregion

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
        Chest.OnChestOpened += ChestUIToggle;

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
        Chest.OnChestOpened -= ChestUIToggle;

        UnsubscribeInventoryUI(inventoryUI);
        UnsubscribeInventoryUI(chestUI);

        if (interactor != null) interactor.TargetChanged -= IiPanelChange;
    }

    void Awake()
    {
        fromStorage = inventoryUI.SlotPanel.Inventory;
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

    void InventoryUIHandleEsc()
    {
        if (IsOpen(inventoryUI.gameObject) || IsOpen(chestUI.gameObject))
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
        if (!IsOpen(inventoryUI.gameObject)) OpenInventoryUI();
        else
        {
            CloseInventoryUI();
            CloseChestUI();
        }
    }

    void OpenInventoryUI()
    {
        inventoryUI.gameObject.SetActive(true);
        SubscribeInventoryUI(inventoryUI);
        CursorController.Apply(false);
    }

    void CloseInventoryUI()
    {
        inventoryUI.gameObject.SetActive(false);
        CloseTooltip();
        CursorController.Apply(true);
    }

    void ChestUIToggle(Chest c)
    {
        if (!IsOpen(chestUI.gameObject))
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
        chestUI.gameObject.SetActive(true);
        chestUI.SetChest(c);
        SubscribeInventoryUI(chestUI);
    }

    void CloseChestUI()
    {
        chestUI.SlotPanel.Clear();
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
    IItemSource fromStorage;
    IItemSink toStorage = null;
    void OnBeginDragFromPanel(InventorySlotUI slotUI, IItemSource storage, PointerEventData e)
    {
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

    void OnDroppedFromPanel(InventorySlotUI slotUI, PointerEventData e)
    {
        StoredItem item = slotUI.EnterItem;
        if (fromStorage == null || item == null || item.itemdata == null) return;

        // 마우스 놓은 Storage 창 구하기(인벤토리, 장비창, 상자)
        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        raycaster.Raycast(e, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                Debug.Log("Dropped 인벤토리 창");
                toStorage = inventoryUI.SlotPanel.Inventory;
            }
            if (result.gameObject.CompareTag("ChestUI"))
            {
                Debug.Log("Dropped 상자 창");
                toStorage = chestUI.ChestInventory;
            }
            if (result.gameObject.CompareTag("EquipUI"))
            {
                Debug.Log("Dropped 장비창");
                toStorage = equipSlotPanel.EquipInventory;
            }
            else
            {
                Debug.Log("Dropped 바닥");
                //toStorage = worldInventory;
            }
        }
        if (toStorage == null)
        {
            return;
        }

        InventoryManager.TryDeliver(fromStorage, toStorage, item);

        // 실패 시 드래그 프리뷰만 닫기
        dragSlot.gameObject.SetActive(false);
    }
    public void OnRightClick(StoredItem item, IItemSource fromStorage)
    {
        if (item == null || fromStorage == null)
            return;

        bool fromEquip = ReferenceEquals(fromStorage, EquipInventory);
        bool fromChest = ReferenceEquals(fromStorage, ChestInventory);
        bool fromInv = ReferenceEquals(fromStorage, PlayerInventory);

        // 1️⃣ 장비창 → 언이퀍 (인벤토리 우선, 실패 시 Chest)
        if (fromEquip)
        {
            IItemSink inv = PlayerInventory;
            IItemSink chest = IsOpen(ChestUI.gameObject) ? ChestInventory : null;

            bool moved = InventoryManager.TryDeliverWithFallbacks(fromStorage, item, inv, chest);
            if (!moved)
                Debug.Log("언이퀍 실패: 인벤토리/상자 공간 없음");
            return;
        }

        // 2️⃣ 상자 → 인벤토리
        else if (fromChest)
        {
            InventoryManager.TryDeliver(fromStorage, PlayerInventory, item);
            return;
        }

        // 3️⃣ 인벤토리 → Chest 우선, 실패 시 Equip(장비 가능)
        else if (fromInv)
        {
            IItemSink chest = IsOpen(ChestUI.gameObject) ? ChestInventory : null;
            IItemSink equip = (item.itemdata != null && item.itemdata.type == ItemType.Weapon) ? EquipInventory : null;

            bool moved = InventoryManager.TryDeliverWithFallbacks(fromStorage, item, chest, equip);
            if (!moved)
                Debug.Log("우클릭 이동 실패: Chest/Equip 모두 실패");
            return;
        }

        // 4️⃣ 기타 → 인벤토리
        else
        {
            InventoryManager.TryDeliver(fromStorage, PlayerInventory, item);
        }
    }

    #endregion
}
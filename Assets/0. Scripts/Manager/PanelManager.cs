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
    #endregion

    #region 유틸 패널 컨트롤러
    TooltipUIController tooltipUIController;
    InteractUIController interactUIController;
    DragSlotUIController dragSlotUIController;
    #endregion


    //이벤트들
    public event Action<StorageTarget, StorageTarget, StoredItem> OnItemDropped;
    public event Action<StoredItem, StorageTarget> OnItemRightClicked;

    // Chest, Npc 등 전역 이벤트 연결
    [SerializeField] InteractionHub hub;

    // 런타임에 활성화되어 저장해야 할 객체들
    Chest curChest;
    NpcMono curNpc;

    GraphicRaycaster raycaster;

    public bool IsOpen(GameObject gameObject)
    {
        return gameObject != null && gameObject.activeSelf;
    }

    void Awake()
    {
        tooltipUIController = GetComponent<TooltipUIController>();
        interactUIController = GetComponent<InteractUIController>();
        dragSlotUIController = GetComponent<DragSlotUIController>();
        raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        //fromStorage = inventoryUI.SlotPanel.Inventory.Core;
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
        interactor.OnInteractorChange += HandleInteractorChange;
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

        interactor.OnInteractorChange -= HandleInteractorChange;
    }

    #region 이벤트 구독 (InventoryUI, ChestUI)
    void SubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) { return; }

        UnsubscribeInventoryUI(inventoryUI);

        inventoryUI.OnMouseEnter += HandleTooltipShow;
        inventoryUI.OnMouseExit += HandleTooltipHide;
        inventoryUI.OnRightClick += HandleRightClick;
        inventoryUI.OnBeginDrag += HandleBeginDragFromPanel;
        inventoryUI.OnDragging += HandleDraggingFromPanel;
        inventoryUI.OnDropped += HandleEndDragFromPanel;
    }

    void UnsubscribeInventoryUI(InventoryUI inventoryUI)
    {
        if (inventoryUI == null) { return; }

        inventoryUI.OnMouseEnter -= HandleTooltipShow;
        inventoryUI.OnMouseExit -= HandleTooltipHide;
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
            hub.npc.OnExit -= HandleNpcExit;
        }
        if (hub != null && hub.chest != null)
        {
            hub.chest.OnOpen -= HandleChestOpen;
            hub.chest.OnClose -= HandleChestClose;
        }
    }

    #endregion

    #region Inventory UI 열고 닫기

    void HandleUIClose()
    {
        CursorController.Apply(true);
    }

    void HandleInventoryUIToggle()
    {
        if (!IsOpen(inventoryUI.gameObject))
        {
            inventoryUI.gameObject.SetActive(true);
        }
        else
        {
            inventoryUI.gameObject.SetActive(false);
            curChest?.ForceCloseFromUI();
        }
    }

    #endregion

    #region Chest UI 열고 닫기
    void HandleChestOpen(Chest c)
    {
        if (curChest != null && curChest != c)
        {
            HandleChestClose(curChest);
        }

        //chest 설정
        curChest = c;
        chestUI.SetChest(c);

        //ui 열기(인벤토리, 상자 모두)
        inventoryUI.gameObject.SetActive(true);
        chestUI.gameObject.SetActive(true);

        //유틸 ui 닫기
        interactUIController.Hide();
    }

    void HandleChestClose(Chest c)
    {
        if (!IsOpen(chestUI.gameObject)) return;

        //chest 비우기
        if (curChest == c) curChest = null;
        chestUI.ClearChest();

        //ui 닫기
        chestUI.gameObject.SetActive(false);
        inventoryUI.gameObject.SetActive(false);
        
        //유틸 ui 다시 활성화
        interactUIController.Show();
    }
    #endregion



    #region 아이템 툴팁 열고 닫기
    void HandleTooltipShow(SlotPanelEventArgs args)
    {
        tooltipUIController.Show(args);
    }

    void HandleTooltipHide(SlotPanelEventArgs args)
    {
        tooltipUIController.Hide();
    }
    #endregion

    #region 아이템 슬롯 드래그 하기 & 아이템 옮기기(인벤토리↔상자)
    void HandleBeginDragFromPanel(SlotPanelEventArgs args)
    {
        StoredItem item = args.Item;
        if (item == null) { return; }


        tooltipUIController.Hide();

        dragSlotUIController.Show(args.Item);
    }

    void HandleDraggingFromPanel(SlotPanelEventArgs args)
    {
        dragSlotUIController.Move(args);
    }

    void HandleEndDragFromPanel(SlotPanelEventArgs args)
    {
        StoredItem item = args.Item;
        if (item == null || item.itemData == null) return;
        if (args.Pointer == null) return;

        StorageTarget fromStorage = args.Source;  // 드래그 시작 지점(인벤/상자/장비)
        StorageTarget toStorage = StorageTarget.None; // 드래그 끝 지점(인벤/상자/장비)

        // 마우스가 놓인 UI 판정
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(args.Pointer, results);

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

        if (toStorage == StorageTarget.None)
        {
            toStorage = StorageTarget.World;
        }

        OnItemDropped?.Invoke(fromStorage, toStorage, item);

        dragSlotUIController.Hide();
    }
    void HandleRightClick(SlotPanelEventArgs args)
    {
        if (args.Item == null || args.Item.itemData == null) { return; }

        Log.Info($"{args.Source}에서 {args.Item.itemData.name}을 우클릭");
        tooltipUIController.Hide();
        // 외부 이벤트는 기존 시그니처 유지
        OnItemRightClicked?.Invoke(args.Item, args.Source);
    }
    #endregion

    #region InteractPanel 열고 닫기
    void HandleInteractorChange(IInteractable interactable)
    {
        if (interactable != null)
        {
            //꺼져 있으면 우선 키기
            if (!interactUIController.IsOpen)
            {
                interactUIController.Show();
            }
            //리프레시
            interactUIController.Refresh(interactable);
        }
        else
        {
            //널이면 닫기
            interactUIController.Hide();
        }
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

        //PushUI(npcUI.gameObject, CloseNpcFromTop, "NPC");
        //npcUI.Bind(npc.Core);
        //BindNpcCore(npc.Core, true);
    }

    void HandleNpcExit(NpcMono npc)
    {
        npcUI.gameObject.SetActive(false);
    }
    #endregion
}
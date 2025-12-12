using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] ShopUI shopUI;
    #endregion

    #region 유틸 패널 컨트롤러
    TooltipUIController tooltipUIController;
    InteractUIController interactUIController;
    DragSlotUIController dragSlotUIController;
    #endregion

    // Chest, Npc 등 전역 이벤트 연결
    [SerializeField] GameEventHub hub;

    // 런타임에 활성화되어 저장해야 할 객체들
    Chest curChest;
    NpcMono curNpc;

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

        // 이벤트 구독용
        uiForwarders = new SlotEventBridge[]
        {
            inventoryUI.SlotEventBridge,
            chestUI.SlotEventBridge,
            shopUI.SlotEventBridge,
            npcUI.SlotEventBridge
        };

    }
    void OnEnable()
    {
        //입력 이벤트(I, Esc)
        InputManager.Instance.OnToggleInventory += HandleInventoryUIToggle;
        InputManager.Instance.OnEsc += HandleEscUIClose;

        //주요 UI 열리고 닫히는거 인식
        NpcUIPresence.OnStateChanged += HandleMouseLock;
        RecountAndApply();


        inventoryUI.gameObject.SetActive(true);
        chestUI.gameObject.SetActive(true);
        shopUI.gameObject.SetActive(true);
        npcUI.gameObject.SetActive(true);
        inventoryUI.gameObject.SetActive(false);
        chestUI.gameObject.SetActive(false);
        shopUI.gameObject.SetActive(false);
        npcUI.gameObject.SetActive(false);

        
        // 이벤트 구독
        SubscribeAllUIs();

        // 전역 이벤트(Chest, Npc)
        SubscribeHubEvent();

        //InteractPanel 이벤트
        interactor.OnInteractorChange += HandleInteractorChange;
    }
    void OnDisable()
    {
        InputManager.Instance.OnToggleInventory -= HandleInventoryUIToggle;
        InputManager.Instance.OnEsc -= HandleEscUIClose;

        NpcUIPresence.OnStateChanged -= HandleMouseLock;

        UnsubscribeAllUIs();
        UnSubscribeHubEvent();

        interactor.OnInteractorChange -= HandleInteractorChange;
    }

    #region 이벤트 구독
    SlotEventBridge[] uiForwarders;
    void SubscribeAllUIs()
    {
        UnsubscribeAllUIs();
        foreach (var f in uiForwarders)
        {
            if (f == null) continue;
            f.MouseEntered += HandleTooltipShow;
            f.MouseExited += HandleTooltipHide;
            f.RightClicked += HandleRightClick;
            f.DragBegan += HandleBeginDragFromPanel;
            f.Dragging += HandleDraggingFromPanel;
            f.DragEnded += HandleEndDragFromPanel;
        }
    }
    void UnsubscribeAllUIs()
    {
        foreach (var f in uiForwarders)
        {
            if (f == null) continue;
            f.MouseEntered -= HandleTooltipShow;
            f.MouseExited -= HandleTooltipHide;
            f.RightClicked -= HandleRightClick;
            f.DragBegan -= HandleBeginDragFromPanel;
            f.Dragging -= HandleDraggingFromPanel;
            f.DragEnded -= HandleEndDragFromPanel;
        }
    }

    // Hub Event (Chest, Npc) 전역 이벤트
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

    #region InventoryUI 열닫
    void HandleInventoryUIToggle()
    {
        if (!IsOpen(inventoryUI.gameObject))
        {
            InventoryUIOpen();
        }
        else
        {
            InventoryUIClose();
        }
    }
    void InventoryUIOpen()
    {
        if(IsOpen(inventoryUI.gameObject)) return;

        inventoryUI.gameObject.SetActive(true);
    }
    void InventoryUIClose()
    {
        if(!IsOpen(inventoryUI.gameObject)) return;

        inventoryUI.gameObject.SetActive(false);
        tooltipUIController.Hide();

        //인벤토리 닫을 때 chest, shop  같이 닫힘
        ShopClose();
        curChest?.ForceCloseFromUI();
    }
    #endregion

    #region ChestUI 열닫
    void HandleChestOpen(Chest c)
    {
        if (curChest != null && curChest != c) HandleChestClose(curChest);

        //ui 열기(인벤토리, 상자 모두)
        InventoryUIOpen();
        chestUI.gameObject.SetActive(true);

        //chest 설정
        curChest = c;
        chestUI.SetChest(c);

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
        InventoryUIClose();

        //유틸 ui 다시 활성화
        interactUIController.Show();
    }
    #endregion

    #region NpcUI 열닫
    void HandleNpcEnter(NpcMono npc)
    {
        if (curNpc != null && curNpc != npc) HandleNpcExit(npc);

        curNpc = npc;
        curNpc.OpenShop += HandleShopOpen;

        npcUI.gameObject.SetActive(true);
        npcUI.Bind(npc.Core.dialogueCore);

        interactUIController.Hide();
    }

    void HandleNpcExit(NpcMono npc)
    {
        if (!IsOpen(npcUI.gameObject)) return;

        ShopClose();
        npcUI.Close();

        curNpc.OpenShop -= HandleShopOpen;
        if (curNpc == npc) curNpc = null;
        
        interactUIController.Show();
    }
    #endregion

    #region ShopUI 열닫
    void HandleShopOpen()
    {
        if (curNpc.ShopInventory == null) return;
        if (IsOpen(shopUI.gameObject)) return;
        shopUI.gameObject.SetActive(true);
        shopUI.Bind(curNpc.ShopInventory, inventoryUI.SlotPanel.Inventory.Core.MyCoin);
        
        InventoryUIOpen();
    }
    void ShopClose()
    {
        if (!IsOpen(shopUI.gameObject)) return;

        InventoryUIClose();
        shopUI.gameObject.SetActive(false);
    }
    #endregion

    #region 툴팁 열닫
    void HandleTooltipShow(ItemUIEventArgs args)
    {
        tooltipUIController.Show(args);
    }

    void HandleTooltipHide(ItemUIEventArgs args)
    {
        tooltipUIController.Hide();
    }
    #endregion

    #region 아이템 옮기기
    void HandleBeginDragFromPanel(ItemUIEventArgs args)
    {
        StoredItem item = args.Item;
        if (item == null) { return; }

        tooltipUIController.Hide();
        dragSlotUIController.Show(args.Item);
    }

    void HandleDraggingFromPanel(ItemUIEventArgs args)
    {
        dragSlotUIController.Move(args);
    }

    void HandleEndDragFromPanel(ItemUIEventArgs args)
    {
        if (args.Item == null || args.Item.itemData == null) return;
        if (args.Pointer == null) return;

        StorageTarget fromStorage = args.Source;
        StorageTarget toStorage = ToStorageTarget(args.Pointer);

        InventoryManager.Instance.HandleItemDropped(fromStorage, toStorage, args.Item);
        dragSlotUIController.Hide();
    }

    void HandleRightClick(ItemUIEventArgs args)
    {
        if (args.Item == null || args.Item.itemData == null) { return; }

        Log.Info("right click panelmanager");
        tooltipUIController.Hide();
        InventoryManager.Instance.HandleRightClick(args.Item, args.Source);
    }
    #endregion

    #region InteractPanel 열닫
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

    #region CursorController
    int enabledUICount = 0;
    // ESC 눌렀을 때
    void HandleEscUIClose()
    {
        // UI가 열려 있으면 닫고 마우스 Free
        if (enabledUICount > 0)
        {
            if(inventoryUI.isActiveAndEnabled) InventoryUIClose();
            if(interactor != null) interactor.InteractExit();
            
            if(enabledUICount == 0) CursorController.Apply(true);
        }
        // UI가 없으면 마우스 Free, Lock 토글
        else
        {
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }

    // UIPresence가 붙은 UI의 이벤트를 감지
    void HandleMouseLock(int i)
    {
        // 열리면 +1, 닫히면 -1
        enabledUICount += i;
        if (enabledUICount < 0) enabledUICount = 0; // 안전장치

        CursorController.Apply(enabledUICount == 0);
    }
    #endregion

    #region 기타 유틸 함수
    // UI 켜져있는 갯수를 Awake에서 초기화
    void RecountAndApply()
    {
        enabledUICount = 0;
        NpcUIPresence[] presences = FindObjectsOfType<NpcUIPresence>(true); // 비활성 포함
        for (int i = 0; i < presences.Length; i++)
        {
            if (presences[i].gameObject.activeInHierarchy)
                enabledUICount++;
        }
        CursorController.Apply(enabledUICount == 0); // 모두 꺼지면 true
    }

    GraphicRaycaster raycaster;
    // EndDrag에서 Storage 판정
    StorageTarget ToStorageTarget(PointerEventData pointer)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointer, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject go = results[i].gameObject;

            if (go.CompareTag("InventoryUI")) return StorageTarget.Player;
            if (go.CompareTag("ChestUI")) return StorageTarget.Chest;
            if (go.CompareTag("ShopUI")) return StorageTarget.Shop;
            if (go.CompareTag("EquipUI")) return StorageTarget.Equip;
        }

        return StorageTarget.World;
    }
    #endregion
}
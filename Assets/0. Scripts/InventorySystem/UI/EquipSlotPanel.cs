using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlotPanel : MonoBehaviour, ISlotPanel
{
    [SerializeField] EquipInventory equipInventory;
    public EquipInventory EquipInventory => equipInventory;


    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    [SerializeField] private InventorySlotUI[] uiSlots;
    [SerializeField] InventorySlotUI weaponSlot => uiSlots[(int)EquipIndex.Weapon];
    //[SerializeField] InventorySlotUI helmetSlot => uiSlots[(int)EquipIndex.Helmet];
    //[SerializeField] InventorySlotUI chestArmorSlot => uiSlots[(int)EquipIndex.ChestArmor];

    void OnEnable()
    {
        SubscribeInventory();
        SubscribeSlotUI();
    }
    void OnDisable()
    {
        UnSubscribeInventory();
        UnSubscribeSlotUI();
    }

    public void Refresh()
    {
        weaponSlot.Bind(equipInventory.Weapon);
    }

    

    void SubscribeInventory()
    {
        UnSubscribeInventory();
        equipInventory.OnChanged += Refresh;
    }
    void UnSubscribeInventory()
    {
        equipInventory.OnChanged -= Refresh;
    }

    #region 인벤토리 슬롯 UI 이벤트 구독
    //모든 인벤토리 슬롯 UI 이벤트 구독
    void SubscribeSlotUI()
    {
        UnSubscribeSlotUI();

        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;

            slot.handler.PointerEnter += HandlePointerEnter;
            slot.handler.PointerExit += HandlePointerExit;
            slot.handler.RightClick += UseItem;
            slot.handler.BeginDragSlot += HandleBeginDrag;
            slot.handler.DragSlot += HandleDrag;
            slot.handler.EndDragSlot += HandleEndDrag;
        }
    }
    void UnSubscribeSlotUI()
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot == null || slot.handler == null) continue;

            slot.handler.PointerEnter -= HandlePointerEnter;
            slot.handler.PointerExit -= HandlePointerExit;
            slot.handler.RightClick -= UseItem;
            slot.handler.BeginDragSlot -= HandleBeginDrag;
            slot.handler.DragSlot -= HandleDrag;
            slot.handler.EndDragSlot -= HandleEndDrag;
        }
    }
    #endregion


    // = InventoryUI의 UseItem. 여기선 그냥 장비 장착 해제
    void UseItem(InventorySlotUI slotUI)
    {
        bool isRemove = equipInventory.TryRemoveItem(slotUI.EnterItem);
        //장착 해제 성공하면 벗은 무기 전달(인벤토리 or Chest or 바닥)
        if(isRemove)
        {
            Refresh();
            //PanelManager 구독
            //UnEquippedItemReturnHandler(slotUI.EnterItem);
        }
    }

    #region 재발행할 이벤트
    public event Action<InventorySlotUI> TooltipShown;
    public event Action<InventorySlotUI> TooltipHidden;
    public event Action<InventorySlotUI, IItemSource, PointerEventData> BeginDrag;
    public event Action<InventorySlotUI, PointerEventData> Dragging;
    public event Action<InventorySlotUI, PointerEventData> Dropped;

    void HandlePointerEnter(InventorySlotUI slotUI)
    {
        TooltipShown?.Invoke(slotUI);
    }

    void HandlePointerExit(InventorySlotUI slotUI)
    {
        TooltipHidden?.Invoke(slotUI);
    }

    void HandleBeginDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        // 드래그 시작 시 툴팁 강제 숨김
        TooltipHidden?.Invoke(slotUI);
        BeginDrag?.Invoke(slotUI, equipInventory, e);
    }

    void HandleDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Dragging?.Invoke(slotUI, e);
    }

    void HandleEndDrag(InventorySlotUI slotUI, PointerEventData e)
    {
        Refresh();
        Dropped?.Invoke(slotUI, e);
    }

    #endregion

}

using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    //인벤토리 슬롯
    [SerializeField] SlotPanel slotPanel;
    public SlotPanel SlotPanel => slotPanel;

    //장비 슬롯
    [SerializeField] EquipSlotPanel equipSlotPanel;
    public EquipSlotPanel EquipSlotPanel => equipSlotPanel;

    //구독 편하게 하기 용
    SlotPanelBase[] panels;

    void Awake()
    {
        panels = GetComponentsInChildren<SlotPanelBase>(true);
    }

    void OnEnable()
    {
        SubscribeSlotPanel();
    }

    private void OnDisable()
    {
        UnsubscribeSlotPanel();
    }

    public void SetSlotPanel(Chest chest)
    {
        slotPanel.SetInventory(chest);
        SubscribeSlotPanel();
    }
    public void ClearSlotPanel()
    {
        
    }

    private void SubscribeSlotPanel()
    {
        if (panels == null)
        {
            NullChecker.NullCheck(this, nameof(panels));
            return;
        }

        UnsubscribeSlotPanel();

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].PointerEnter += OnPointerEnter;
            panels[i].PointerExit += OnPointerExit;
            panels[i].RightClick += OnPointerRightClick;
            panels[i].BeginDrag += OnBeginDrag;
            panels[i].Dragging += OnDragging;
            panels[i].EndDrag += OnEndDrag;
        }

    }

    private void UnsubscribeSlotPanel()
    {
        if (panels == null)
        {
            NullChecker.NullCheck(this, nameof(panels));
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].PointerEnter -= OnPointerEnter;
            panels[i].PointerExit -= OnPointerExit;
            panels[i].RightClick -= OnPointerRightClick;
            panels[i].BeginDrag -= OnBeginDrag;
            panels[i].Dragging -= OnDragging;
            panels[i].EndDrag -= OnEndDrag;
        }
    }


    #region  ── 위로 포워딩할 이벤트 (PanelManager에서 구독) ──
    public event Action<InventorySlotUI> PointerEnter;
    public event Action PointerExit;
    public event Action<StoredItem, IItemSource> RightClick;
    public event Action<StoredItem, IItemSource, PointerEventData> BeginDrag;
    public event Action<PointerEventData> Dragging;
    public event Action<StoredItem, PointerEventData> EndDrag;



    void OnPointerEnter(InventorySlotUI slotUI) => PointerEnter?.Invoke(slotUI);
    void OnPointerExit() => PointerExit?.Invoke();
    public void OnPointerRightClick(StoredItem item, IItemSource fromStorage) => RightClick?.Invoke(item, fromStorage);
    
    void OnBeginDrag(StoredItem item, IItemSource source, PointerEventData e)
    {
        PointerExit?.Invoke();
        BeginDrag?.Invoke(item, source, e);
    }

    void OnDragging(PointerEventData e) => Dragging?.Invoke(e);

    void OnEndDrag(StoredItem item, PointerEventData e) => EndDrag?.Invoke(item, e);
    #endregion
}
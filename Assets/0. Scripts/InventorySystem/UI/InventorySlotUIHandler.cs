using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotUIHandler : MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler,
IPointerClickHandler,
IBeginDragHandler,
IDragHandler,
IEndDragHandler
{
    InventorySlotUI mySlot;

    void Awake()
    {
        mySlot = gameObject.GetComponent<InventorySlotUI>();
    }

    #region 이벤트 발행
    public event Action<InventorySlotUI> PointerEnter;
    public event Action PointerExit;
    public event Action<InventorySlotUI> LeftClick;
    public event Action<StoredItem> RightClick;
    public event Action<StoredItem, PointerEventData> BeginDrag;
    public event Action<PointerEventData> Dragging;
    public event Action<StoredItem, PointerEventData> EndDrag;


    // 툴팁 띄우기
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 

        PointerEnter?.Invoke(mySlot);
    }

    // 툴팁 숨기기
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 
        
        PointerExit?.Invoke();
    }

    // 아이템 사용, 장착(우클릭)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClick?.Invoke(mySlot);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClick?.Invoke(mySlot.EnterItem);
        }
    }

    //아이템 옮기기 1
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 

        mySlot.Invisible();
        BeginDrag?.Invoke(mySlot.EnterItem, eventData);
    }

    //아이템 옮기기 2
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 

        Dragging?.Invoke(eventData);
    }

    //아이템 옮기기 3
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (mySlot.EnterItem == null || mySlot.EnterItem.itemData == null) return; 

        mySlot.Visible();
        EndDrag?.Invoke(mySlot.EnterItem, eventData);
    }
    #endregion
}

using System;
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
    public event Action<InventorySlotUI> PointerExit;
    public event Action<InventorySlotUI> LeftClick;
    public event Action<InventorySlotUI> RightClick;
    public event Action<InventorySlotUI, PointerEventData> BeginDragSlot;
    public event Action<InventorySlotUI, PointerEventData> DragSlot;
    public event Action<InventorySlotUI, PointerEventData> EndDragSlot;


    // 툴팁 띄우기
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(mySlot);
    }

    // 툴팁 숨기기
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(mySlot);
    }
    public void HideTooltip() {PointerExit?.Invoke(mySlot);}
    
    // 아이템 사용, 장착(우클릭)
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClick?.Invoke(mySlot);
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            RightClick?.Invoke(mySlot);
        }
    }

    //아이템 옮기기 1
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        mySlot.Invisible();
        BeginDragSlot?.Invoke(mySlot, eventData);
    }

    //아이템 옮기기 2
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        DragSlot?.Invoke(mySlot, eventData);
    }

    //아이템 옮기기 3
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        mySlot.Visible();
        EndDragSlot?.Invoke(mySlot, eventData);
    }
    #endregion
}
